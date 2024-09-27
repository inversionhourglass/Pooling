namespace SingleFeatureCases.PoolItems.Patterns
{
    public class CustomObjectTryReset : NotResettablePoolItem
    {
        public override bool ExceptedReset => true;

        public object? ObjectReset()
        {
            Reset = true;
            return null;
        }
    }
}
