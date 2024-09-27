namespace SingleFeatureCases.PoolItems
{
    public interface IResetState
    {
        bool Reset { get; }

        void CheckResetState();
    }
}
