using ConsoleApp.Models;
using DependencyInjection.StaticAccessor;
using Microsoft.Extensions.DependencyInjection;
using Pooling;

namespace ConsoleApp.Pools
{
    public class ServiceSetupPool : IPool<Service1>
    {
        public Service1 Get()
        {
            var service1 = new Service1();
            var service2 = PinnedScope.ScopedServices?.GetService<Service2>();
            service1.Service2 = service2;

            return service1;
        }

        public void Return(Service1 value) { }
    }
}
