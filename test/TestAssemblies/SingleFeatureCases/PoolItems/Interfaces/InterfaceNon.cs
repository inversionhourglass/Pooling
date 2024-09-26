namespace SingleFeatureCases.PoolItems.Interfaces
{
    /// <summary>
    /// 通过<see cref="ExclusivePattern"/>排除所有类型，该类型永远不会池化
    /// </summary>
    public class InterfaceNon : PoolItem
    {
        public override string ExclusivePattern => "execution(* *(..))";
    }
}
