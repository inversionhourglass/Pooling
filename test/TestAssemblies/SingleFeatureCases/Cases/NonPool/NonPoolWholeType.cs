using Pooling;
using SingleFeatureCases.PoolItems;

namespace SingleFeatureCases.Cases.NonPool
{
    [NonPooled]
    partial class NonPoolWholeType
    {
        private static void SetPoolingResult(PoolingResult pooling, IPoolingState any, IPoolingState non, IPoolingState nonPattern, IPoolingState nonTypes)
        {
            pooling.ShouldPooled();
            pooling.ShouldNotPooled(any, non, nonPattern, nonTypes);
            PoolingResult = pooling;
        }
    }
}
