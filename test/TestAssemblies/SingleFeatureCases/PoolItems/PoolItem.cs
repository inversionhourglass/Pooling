using Pooling;

namespace SingleFeatureCases.PoolItems
{
    public abstract class PoolItem : NotResettablePoolItem, IPoolItem
    {
        public override bool ExceptedReset => true;

        public virtual bool TryReset()
        {
            Reset = true;
            return true;
        }
    }
}
