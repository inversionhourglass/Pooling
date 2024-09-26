using SingleFeatureCases.PoolItems;

namespace SingleFeatureCases.Cases.Interfaces.II
{
    partial class NonPattern2
    {
        private static void SetPoolingResult(PoolingResult pooling, IPoolingState any, IPoolingState non, IPoolingState nonPattern, IPoolingState nonTypes)
        {
            pooling.ShouldPooled(any, nonTypes);
            pooling.ShouldNotPooled(non, nonPattern);
            PoolingResult = pooling;
        }
    }
}
