using ConsoleApp.Pools;
using Pooling;

namespace ConsoleApp.Cases
{
    public class ThreadLocalPoolCase
    {
        public static void Test()
        {
            Pool<Random>.Set(new ThreadLocalPool<Random>());

            Console.WriteLine("=====================线程单例测试=====================");
            Console.WriteLine();
            var map = new Dictionary<int, Random>();
            var i = 0L;

            while (Interlocked.Read(ref i) == 0)
            {
                Task.Run(() =>
                {
                    var random = new Random();
                    if (map.TryGetValue(Environment.CurrentManagedThreadId, out var value))
                    {
                        if (random == value)
                        {
                            Console.WriteLine("线程单例测试成功");
                        }
                        else
                        {
                            Console.WriteLine("线程单例测试失败");
                        }
                        Interlocked.Exchange(ref i, 1);
                    }
                    else
                    {
                        map.Add(Environment.CurrentManagedThreadId, random);
                    }
                }).Wait();
            }

            Console.WriteLine();
        }
    }
}
