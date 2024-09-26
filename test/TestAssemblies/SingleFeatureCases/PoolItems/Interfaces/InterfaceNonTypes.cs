using SingleFeatureCases.Cases.Interfaces.I;
using SingleFeatureCases.Cases.Interfaces.II;
using System;

namespace SingleFeatureCases.PoolItems.Interfaces
{
    /// <summary>
    /// 通过<see cref="ExclusiveTypes"/>排除<see cref="NonTypes1"/>和<see cref="NonTypes2"/>
    /// </summary>
    internal class InterfaceNonTypes : PoolItem
    {
        public override Type[]? ExclusiveTypes => [typeof(NonTypes1), typeof(NonTypes2)];
    }
}
