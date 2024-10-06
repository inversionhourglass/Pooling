using ConsoleApp.Models;
using ConsoleApp.Pools;
using DependencyInjection.StaticAccessor;
using Microsoft.Extensions.DependencyInjection;
using Pooling;

namespace ConsoleApp.Cases
{
    public class ServiceSetupPoolCase
    {
        public static void Test()
        {
            Pool<Service1>.Set(new ServiceSetupPool());

            Console.WriteLine("=====================附加初始化测试=====================");
            Console.WriteLine();

            var factory = ServiceProviderFactoryBuilder
                            .CreateDefault()
                            .Add(new ServiceScopeFactoryPinnedReplacer())
                            .Build();
            var services = new ServiceCollection();
            services.AddScoped<Service2>();
            var provider = factory.CreateServiceProvider(services);

            using (var scope = provider.CreateScope())
            {
                var service1 = new Service1();
                if (service1.Service2 == null)
                {
                    Console.WriteLine("附加初始化测试失败");
                }
                else
                {
                    Console.WriteLine("附加初始化测试成功");
                }
            }

            Console.WriteLine();
        }
    }
}
