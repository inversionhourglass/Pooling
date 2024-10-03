using Pooling;
using SingleFeatureCases.Cases.Interfaces.I;
using SingleFeatureCases.Cases.Interfaces.II;

namespace SingleFeatureCases.PoolItems.Interfaces
{
    /// <summary>
    /// 通过<see cref="PoolingExclusiveAttribute"/>排除<see cref="NonTypes1"/>和<see cref="NonTypes2"/>
    /// </summary>
    [PoolingExclusive(Types = [typeof(NonTypes1), typeof(NonTypes2)])]
    public class InterfaceNonTypes : PoolItem
    {
    }
}
