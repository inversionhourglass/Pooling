using SingleFeatureCases.PoolItems.Patterns;

namespace SingleFeatureCases.Cases.ConfiguredPattern.I
{
    public class NotInspectExceptedMicrosoft : Stateful<NotInspectExceptedMicrosoft>
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

            pooling.ShouldPooled(statelessItem, customBoolReset, customObjectReset, customVoidReset);
            pooling.ShouldNotPooled(microsoftItem);
            PoolingResult = pooling;
        }
    }
}
