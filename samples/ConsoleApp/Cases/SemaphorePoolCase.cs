using ConsoleApp.Pools;
using Pooling;

namespace ConsoleApp.Cases
{
    public class SemaphorePoolCase
    {
        public static void Test()
        {
            Pool<HttpClient>.Set(new SemaphorePool<HttpClient>());
            
            var clients = new List<HttpClient>();

            Console.WriteLine("=====================信号量测试=====================");
            Console.WriteLine();
            var i = 0;
            for (; i < 10; i++)
            {
                var client = new HttpClient();
                if (client == null)
                {
                    Console.WriteLine("信号量达到峰值，测试成功");
                    break;
                }
                clients.Add(client);
            }

            if (i == 10)
            {
                Console.WriteLine("信号量无效，测试失败");
            }

            Console.WriteLine();
        }
    }
}
