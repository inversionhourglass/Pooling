using Pooling;
using SingleFeatureCases.PoolItems.Interfaces;

namespace SingleFeatureCases.Cases.NonPool
{
    public class NonPoolWholeMethod : Stateful<NonPoolWholeMethod>
    {
        [NonPooled]
        public void NonPool()
        {
            var pooling = PoolingResult.New();

            var any = new InterfaceAny();
            var non = new InterfaceNon();
            var nonPattern = new InterfaceNonPattern();
            var nonTypes = new InterfaceNonTypes();
            AvoidOptimize(any, non, nonPattern, nonTypes);

            pooling.ShouldPooled();
            pooling.ShouldNotPooled(any, non, nonPattern, nonTypes);
            PoolingResult = pooling;
        }

        public void Pooled()
        {
            var pooling = PoolingResult.New();

            var any = new InterfaceAny();
            var non = new InterfaceNon();
            var nonPattern = new InterfaceNonPattern();
            var nonTypes = new InterfaceNonTypes();
            AvoidOptimize(any, non, nonPattern, nonTypes);

            pooling.ShouldPooled(any, nonPattern, nonTypes);
            pooling.ShouldNotPooled(non);
            PoolingResult = pooling;
        }
    }
}
