using System;

namespace Pooling
{
    /// <summary>
    /// Can only be applied to the class that implements <see cref="IPoolItem"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class PoolingExclusiveAttribute : Attribute
    {
        /// <summary>
        /// Which types you do not want to pool this item in.
        /// </summary>
        public Type[]? Types { get; set; }

        /// <summary>
        /// AspctN pattern. Which methods you do not want to pool this item in.
        /// </summary>
        public string? Pattern { get; set; }
    }
}
