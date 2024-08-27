using System;
using System.Threading;

namespace Pooling
{
    /// <summary>
    /// </summary>
    public class Pool
    {
        /// <summary>
        /// </summary>
        protected static IPool? _GenericPool;
        /// <summary>
        /// </summary>
        protected static int _GenericInUse = 0;

        /// <summary>
        /// The maximum number of objects to retain in the pool.
        /// </summary>
        public static int GenericMaximumRetained;

        /// <summary>
        /// Set the generic pool instance.
        /// </summary>
        /// <exception cref="PoolingException">Throws if the generic pool is already in use.</exception>
        public static void Set(IPool pool)
        {
            if (Interlocked.Exchange(ref _GenericInUse, 1) == 1) throw new PoolingException($"The generic pool is already in use and cannot be changed now. You should do this when the application warms up.");

            _GenericPool = pool;
        }
    }

    /// <summary>
    /// </summary>
    public class Pool<T> : Pool where T : class, new()
    {
        //private static ConcurrentDictionary<Type, Pool>
        private static IPool<T>? _Pool;
        private static int _InUse = 0;

        /// <summary>
        /// The maximum number of objects to retain in the pool.
        /// </summary>
        public static int MaximumRetained;

        /// <summary>
        /// Set the pool instance
        /// </summary>
        /// <exception cref="PoolingException">Throws if the pool is already in use.</exception>
        public static void Set(IPool<T> pool)
        {
            if (Interlocked.Exchange(ref _InUse, 1) == 1) throw new PoolingException($"The pool is already in use and cannot be changed now. You should do this when the application warms up.");

            _Pool = pool;
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public static T Get()
        {
            Interlocked.Exchange(ref _InUse, 1);
            Interlocked.Exchange(ref _GenericInUse, 1);

            var pool = _Pool;
            if (pool == null)
            {
                if (_GenericPool != null)
                {
                    return _GenericPool.Get<T>();
                }

                pool = new DefaultPool<T>();
                var savedPool = Interlocked.CompareExchange(ref _Pool, pool, null);
                if (savedPool != null) pool = savedPool;
            }

            return pool.Get();
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public static void Return(T value, Func<T, bool>? resetFunc)
        {
            Interlocked.Exchange(ref _InUse, 1);
            Interlocked.Exchange(ref _GenericInUse, 1);

            var pool = _Pool;
            if (pool == null)
            {
                if (_GenericPool != null)
                {
                    _GenericPool.Return(value, resetFunc);
                    return;
                }

                pool = new DefaultPool<T>();
                var savedPool = Interlocked.CompareExchange(ref _Pool, pool, null);
                if (savedPool != null) pool = savedPool;
            }

            pool.Return(value, resetFunc);
        }
    }
}
