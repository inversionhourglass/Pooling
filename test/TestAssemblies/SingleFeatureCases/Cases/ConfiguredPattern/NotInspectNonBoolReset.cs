using SingleFeatureCases.PoolItems.Patterns;

namespace SingleFeatureCases.Cases.ConfiguredPattern
{
    public class NotInspectNonBoolReset : Stateful<NotInspectNonBoolReset>
    {
        public static void M()
        {
            var pooling = PoolingResult.New();

            var microsoftItem = new MicrosoftPoolItem();
            var statelessItem = new StatelessPoolItem();
            var customBoolReset = new CustomBoolTryReset();
            var customObjectReset = new CustomObjectTryReset();
            var customVoidReset = new CustomVoidTryReset();

            AvoidOptimize(microsoftItem, statelessItem, customBoolReset, customObjectReset, customVoidReset);

            pooling.ShouldPooled(statelessItem, customBoolReset);
            pooling.ShouldNotPooled(microsoftItem, customObjectReset, customVoidReset);
            PoolingResult = pooling;
        }
    }
}
