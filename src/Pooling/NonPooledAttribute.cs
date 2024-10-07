using System;

namespace Pooling
{
    /// <summary>
    /// Do not pool the types under this attribute-applied scope. If <see cref="Types"/> and <see cref="Pattern"/> are both null, all types are not pooled.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor)]
    public sealed class NonPooledAttribute : Attribute
    {
        /// <summary>
        /// Which types do you not want to pool under this attribute-applied scope.
        /// </summary>
        public Type[]? Types { get; set; }

        /// <summary>
        /// AspectN type pattern. Which types do you not want to pool under this attribute-applied scope.
        /// </summary>
        public string? Pattern { get; set; }
    }
}
