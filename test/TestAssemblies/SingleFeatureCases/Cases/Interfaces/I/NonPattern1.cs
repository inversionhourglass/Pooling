using SingleFeatureCases.PoolItems;

namespace SingleFeatureCases.Cases.Interfaces.I
{
    partial class NonPattern1
    {
        private static void SetPoolingResult(PoolingResult pooling, IPoolingState any, IPoolingState non, IPoolingState nonPattern, IPoolingState nonTypes)
        {
            pooling.ShouldPooled(any, nonTypes);
            pooling.ShouldNotPooled(non, nonPattern);
            PoolingResult = pooling;
        }
    }
}
