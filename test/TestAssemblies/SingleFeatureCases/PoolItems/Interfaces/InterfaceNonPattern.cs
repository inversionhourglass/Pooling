namespace SingleFeatureCases.PoolItems.Interfaces
{
    /// <summary>
    /// 通过<see cref="ExclusivePattern"/>排除名称以NonPattern开头的类型
    /// </summary>
    public class InterfaceNonPattern : PoolItem
    {
        public override string? ExclusivePattern => "execution(* NonPattern*.*(..))";
    }
}
