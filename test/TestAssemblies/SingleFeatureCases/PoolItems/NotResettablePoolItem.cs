namespace SingleFeatureCases.PoolItems
{
    public abstract class NotResettablePoolItem : IPoolingState, IResetState
    {
        public bool Reset { get; protected set; }

        public virtual bool ExceptedReset => false;

        public virtual PoolingState State { get; set; }

        public virtual void CheckResetState()
        {
            if (Reset != ExceptedReset) throw new IncorrectResetStateException(!Reset, this.GetType());
        }
    }
}
