using SingleFeatureCases.PoolItems;
using SingleFeatureCases.PoolItems.Patterns;

namespace SingleFeatureCases.Cases.ConfiguredPattern
{
    public class AllInspect : Stateful<AllInspect>
    {
        public static IResetState[] M()
        {
            var pooling = PoolingResult.New();

            var microsoftItem = new MicrosoftPoolItem();
            var statelessItem = new StatelessPoolItem();
            var customBoolReset = new CustomBoolTryReset();
            var customObjectReset = new CustomObjectTryReset();
            var customVoidReset = new CustomVoidTryReset();

            AvoidOptimize(microsoftItem, statelessItem, customBoolReset, customObjectReset, customVoidReset);

            pooling.ShouldPooled(statelessItem, customBoolReset, customObjectReset, customVoidReset);
            // MicrosoftPoolItem为了验证ObjectPool的TryReset会被调用，所以使用了Pooling默认的对象池，所以不会设置PoolingResult
            pooling.ShouldNotPooled(microsoftItem);
            PoolingResult = pooling;

            return [microsoftItem, statelessItem, customBoolReset, customObjectReset, customVoidReset];
        }
    }
}
