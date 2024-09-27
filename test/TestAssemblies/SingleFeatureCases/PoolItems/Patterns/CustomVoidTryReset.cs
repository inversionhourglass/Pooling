namespace SingleFeatureCases.PoolItems.Patterns
{
    public class CustomVoidTryReset : NotResettablePoolItem
    {
        public override bool ExceptedReset => true;

        public void VoidReset()
        {
            Reset = true;
        }
    }
}
