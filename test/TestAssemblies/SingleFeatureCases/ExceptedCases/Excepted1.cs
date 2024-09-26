using SingleFeatureCases.Cases;
using SingleFeatureCases.PoolItems.Interfaces;

namespace SingleFeatureCases.ExceptedCases
{
    public class Excepted1 : Stateful<Excepted1>
    {
        public static void M()
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
