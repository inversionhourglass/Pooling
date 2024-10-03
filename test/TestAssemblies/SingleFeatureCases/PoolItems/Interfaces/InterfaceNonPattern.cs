using Pooling;

namespace SingleFeatureCases.PoolItems.Interfaces
{
    /// <summary>
    /// 通过<see cref="PoolingExclusiveAttribute"/>排除名称以NonPattern开头的类型
    /// </summary>
    [PoolingExclusive(Pattern = "execution(* NonPattern*.*(..))")]
    public class InterfaceNonPattern : PoolItem
    {
    }
}
