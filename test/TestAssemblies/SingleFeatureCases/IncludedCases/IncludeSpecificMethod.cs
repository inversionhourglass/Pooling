using SingleFeatureCases.Cases;
using SingleFeatureCases.PoolItems.Interfaces;

namespace SingleFeatureCases.IncludedCases
{
    public class IncludeSpecificMethod : Stateful<IncludeSpecificMethod>
    {
        public static void Included()
        {
            var pooling = PoolingResult.New();

            var any = new InterfaceAny();
            AvoidOptimize(any);

            pooling.ShouldPooled(any);
            pooling.ShouldNotPooled();
            PoolingResult = pooling;
        }

        public static void NotInclude()
        {
            var pooling = PoolingResult.New();

            var any = new InterfaceAny();
            AvoidOptimize(any);

            pooling.ShouldPooled();
            pooling.ShouldNotPooled(any);
            PoolingResult = pooling;
        }
    }
}
