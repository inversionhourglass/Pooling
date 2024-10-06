using ConsoleApp.Pools;
using Pooling;

namespace ConsoleApp.Models
{
    [PoolingExclusive(Types = [typeof(ServiceSetupPool)])]
    public class Service1 : IPoolItem
    {
        public Service2? Service2 { get; set; }

        public bool TryReset() => true;
    }
}
