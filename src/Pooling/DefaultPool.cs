﻿using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;

namespace Pooling
{
    /// <summary>
    /// <see cref="IPool{T}"/> defualt implementation.
    /// </summary>
    public class DefaultPool<T> : IPool<T>, IDisposable where T : class, new()
    {
        private readonly Lazy<Func<T, bool>> _resetFunc;
        private readonly int _maxCapacity;
        private int _numItems;

        private protected readonly ConcurrentQueue<T> _items = new();
        private protected T? _fastItem;

        private volatile bool _isDisposed;

        /// <summary>
        /// </summary>
        public DefaultPool()
        {
            _resetFunc = new(ResolveReset);
            var formatName = typeof(T).FullName.Replace('.', '_');
            var envMaximumRetained = Environment.GetEnvironmentVariable($"NET_POOLING_MAX_RETAIN_{formatName}");
            envMaximumRetained ??= Environment.GetEnvironmentVariable("NET_POOLING_MAX_RETAIN");

            if (envMaximumRetained == null || !int.TryParse(envMaximumRetained, out var maximumRetained))
            {
                maximumRetained = Pool<T>.MaximumRetained != 0 ? Pool<T>.MaximumRetained : (Pool.GenericMaximumRetained != 0 ? Pool.GenericMaximumRetained : Environment.ProcessorCount * 2);
            }

            _maxCapacity = maximumRetained - 1;
        }

        /// <summary>
        /// Get <typeparamref name="T"/> instance from the pool.
        /// </summary>
        public T Get()
        {
            if (_isDisposed) throw new ObjectDisposedException(GetType().Name);

            var item = _fastItem;
            if (item == null || Interlocked.CompareExchange(ref _fastItem, null, item) != item)
            {
                if (_items.TryDequeue(out item))
                {
                    Interlocked.Decrement(ref _numItems);
                    return item;
                }

                return new();
            }

            return item;
        }

        /// <summary>
        /// Return <paramref name="value"/> back to the pool.
        /// </summary>
        public void Return(T value)
        {
            if (!_isDisposed && TryReset(value))
            {
                if (_fastItem != null || Interlocked.CompareExchange(ref _fastItem, value, null) != null)
                {
                    if (Interlocked.Increment(ref _numItems) <= _maxCapacity)
                    {
                        _items.Enqueue(value);
                    }
                    else
                    {
                        Interlocked.Decrement(ref _numItems);
                    }
                }
            }

            DisposeItem(value);
        }

        private bool TryReset(T value)
        {
            if (value is IPoolItem poolItem) return poolItem.TryReset();

            return _resetFunc.Value(value);
        }

        private Func<T, bool> ResolveReset()
        {
            var type = typeof(T);
            if (type.GetInterface("Microsoft.Extensions.ObjectPool.IResettable") != null)
            {
                var tryReset = type.GetMethod("TryReset", BindingFlags.Instance | BindingFlags.Public);
                if (tryReset != null)
                {
                    return (Func<T, bool>)tryReset.CreateDelegate(typeof(Func<T, bool>));
                }
            }

            return SucceedReset;

            bool SucceedReset(T value) => true;
        }

        /// <summary>
        /// Call <see cref="IDisposable.Dispose"/> for each pool item if it implements <see cref="IDisposable"/>.
        /// </summary>
        public void Dispose()
        {
            _isDisposed = true;

            DisposeItem(_fastItem);
            _fastItem = null;

            while (_items.TryDequeue(out var item))
            {
                DisposeItem(item);
            }
        }

        private static void DisposeItem(T? item)
        {
            if (item is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
