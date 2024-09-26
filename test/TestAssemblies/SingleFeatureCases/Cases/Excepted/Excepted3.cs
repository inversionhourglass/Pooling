using SingleFeatureCases.PoolItems.Interfaces;

namespace SingleFeatureCases.Cases.Excepted
{
    public class Excepted3 : Stateful<Excepted3>
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
