using SingleFeatureCases.PoolItems;

namespace SingleFeatureCases.Cases.Interfaces.I
{
    partial class Other1
    {
        private static void SetPoolingResult(PoolingResult pooling, IPoolingState any, IPoolingState non, IPoolingState nonPattern, IPoolingState nonTypes)
        {
            pooling.ShouldPooled(any, nonPattern, nonTypes);
            pooling.ShouldNotPooled(non);
            PoolingResult = pooling;
        }
    }
}
