namespace SingleFeatureCases.PoolItems.Patterns
{
    public class StatelessPoolItem : NotResettablePoolItem
    {
        public bool TryReset()
        {
            Reset = true;
            return true;
        }
    }
}
