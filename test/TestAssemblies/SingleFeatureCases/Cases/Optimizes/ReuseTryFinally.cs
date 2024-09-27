using SingleFeatureCases.PoolItems.Interfaces;
using System.Diagnostics;

namespace SingleFeatureCases.Cases.Optimizes
{
    public class ReuseTryFinally : Stateful<ReuseTryFinally>
    {
        public static void M()
        {
            try
            {
                var pooling = PoolingResult.New();

                var any = new InterfaceAny();
                AvoidOptimize(any);

                pooling.ShouldPooled(any);
                pooling.ShouldNotPooled();
                PoolingResult = pooling;
            }
            finally
            {
                Debug.Write("finally");
            }
        }
    }
}
