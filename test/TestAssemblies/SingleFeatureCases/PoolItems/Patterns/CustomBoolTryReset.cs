namespace SingleFeatureCases.PoolItems.Patterns
{
    public class CustomBoolTryReset : NotResettablePoolItem
    {
        public override bool ExceptedReset => true;

        public bool BoolReset()
        {
            Reset = true;
            return true;
        }
    }
}
