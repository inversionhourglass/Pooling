using Pooling;

namespace SingleFeatureCases.PoolItems.Interfaces
{
    /// <summary>
    /// 通过<see cref="PoolingExclusiveAttribute"/>排除所有类型，该类型永远不会池化
    /// </summary>
    [PoolingExclusive(Pattern = "execution(* *(..))")]
    public class InterfaceNon : PoolItem
    {
    }
}
