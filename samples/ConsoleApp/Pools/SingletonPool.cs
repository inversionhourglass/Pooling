using Pooling;

namespace ConsoleApp.Pools
{
    public class SingletonPool<T> : IPool<T> where T : class, new()
    {
        // 方便测试才这么写，使用过程中private readonly就可以了
        public static readonly T Value = new();

        public T Get() => Value;

        public void Return(T value) { }
    }
}
