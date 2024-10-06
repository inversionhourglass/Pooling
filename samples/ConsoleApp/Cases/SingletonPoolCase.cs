using ConsoleApp.Pools;
using Pooling;
using System.Collections.Concurrent;

namespace ConsoleApp.Cases
{
    public class SingletonPoolCase
    {
        public static async Task TestAsync()
        {
            Pool<ConcurrentDictionary<Type, object>>.Set(new SingletonPool<ConcurrentDictionary<Type, object>>());

            Sync();
            await Async();
            
            var map = SingletonPool<ConcurrentDictionary<Type, object>>.Value;

            Console.WriteLine("=====================单例测试=====================");
            Console.WriteLine();
            Console.WriteLine("预期输出：(Int32, 1);(String, Sync)");
            Console.WriteLine("实际输出：" + string.Join(';', map.Select(x => $"({x.Key.Name}, {x.Value})")));
            Console.WriteLine();
        }

        public static void Sync()
        {
            var map = new ConcurrentDictionary<Type, object>();
            map[typeof(string)] = nameof(Sync);
        }

        public static async ValueTask Async()
        {
            var map = new ConcurrentDictionary<Type, object>();
            await Task.Yield();
            map[typeof(int)] = 1;
        }
    }
}
