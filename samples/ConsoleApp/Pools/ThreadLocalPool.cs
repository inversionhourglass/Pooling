using Pooling;

namespace ConsoleApp.Pools
{
    public class ThreadLocalPool<T> : IPool<T> where T : class, new()
    {
        private readonly ThreadLocal<T> _random = new(() => new());

        public T Get() => _random.Value!;

        public void Return(T value) { }
    }
}
