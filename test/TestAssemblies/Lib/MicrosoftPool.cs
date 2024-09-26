using Microsoft.Extensions.ObjectPool;
using Pooling;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Lib
{
    public class MicrosoftPool : IPool
    {
        private static readonly DefaultObjectPoolProvider _PoolProvider = new DefaultObjectPoolProvider();
        private static readonly ConcurrentDictionary<Type, object> _Pools = [];

        public T Get<T>() where T : class, new()
        {
            var pool = GetPool<T>();
            return pool.Get();
        }

        public void Return<T>(T value) where T : class, new()
        {
            var pool = GetPool<T>();
            pool.Return(value);
        }

        private ObjectPool<T> GetPool<T>() where T : class, new()
        {
            return (ObjectPool<T>)_Pools.GetOrAdd(typeof(T), t => _PoolProvider.Create(new DefaultPooledObjectPolicy<T>()));
        }
    }
}
