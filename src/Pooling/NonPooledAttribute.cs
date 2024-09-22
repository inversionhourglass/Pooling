using System;

namespace Pooling
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor)]
    public sealed class NonPooledAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public Type[]? Types { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? Pattern { get; set; }
    }
}
