using SingleFeatureCases.PoolItems;

namespace SingleFeatureCases.Cases.Interfaces.I
{
    partial class NonTypes1
    {
        private static void SetPoolingResult(PoolingResult pooling, IPoolingState any, IPoolingState non, IPoolingState nonPattern, IPoolingState nonTypes)
        {
            pooling.ShouldPooled(any, nonPattern);
            pooling.ShouldNotPooled(non, nonTypes);
            PoolingResult = pooling;
        }
    }
}
