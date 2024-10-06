using Pooling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp.Pools
{
    public class SemaphorePool<T> : IPool<T> where T : class, new()
    {
        private readonly Semaphore _semaphore = new(3, 3);
        private readonly DefaultPool<T> _pool = new();

        public T? Get()
        {
            if (!_semaphore.WaitOne(100)) return null;

            return _pool.Get();
        }

        public void Return(T value)
        {
            _pool.Return(value);
            _semaphore.Release();
        }
    }
}
