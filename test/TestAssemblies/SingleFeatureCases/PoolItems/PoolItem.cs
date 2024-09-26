using Pooling;
using System;

namespace SingleFeatureCases.PoolItems
{
    public abstract class PoolItem : IPoolItem, IPoolingState
    {
        public virtual PoolingState State { get; set; }

        public virtual Type[]? ExclusiveTypes { get; }

        public virtual string? ExclusivePattern { get; }

        public bool TryReset()
        {
            return true;
        }
    }
}
