using System;

namespace SingleFeatureCases.PoolItems
{
    public abstract class NotResettablePoolItem : IPoolingState, IResetState
    {
        public bool Reset { get; protected set; }

        public virtual bool ExceptedReset => false;

        public virtual PoolingState State { get; set; }

        public virtual Type[]? ExclusiveTypes { get; }

        public virtual string? ExclusivePattern { get; }

        public virtual void CheckResetState()
        {
            if (Reset != ExceptedReset) throw new IncorrectResetStateException(!Reset, this.GetType());
        }
    }
}
