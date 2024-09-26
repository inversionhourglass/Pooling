using System;

namespace Pooling
{
    /// <summary>
    /// Automatically reset the item that implements this interface when it returns to the pool.
    /// </summary>
    public interface IPoolItem
    {
        /// <summary>
        /// Which types you do not want to pool.
        /// </summary>
        Type[]? ExclusiveTypes { get; }

        /// <summary>
        /// AspctN pattern. Which methods you do not want to pool.
        /// </summary>
        string? ExclusivePattern { get; }

        /// <summary>
        /// Reset the object to a neutral state, semantically similar to when the object was first constructed.
        /// </summary>
        /// <returns>true if the object was able to reset itself, otherwise false</returns>
        bool TryReset();
    }
}
