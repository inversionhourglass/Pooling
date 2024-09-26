using SingleFeatureCases.Cases;
using SingleFeatureCases.PoolItems.Interfaces;

namespace SingleFeatureCases.ExceptedCases.I
{
    public class Excepted2 : Stateful<Excepted2>
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
