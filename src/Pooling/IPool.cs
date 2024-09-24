namespace Pooling
{
    /// <summary>
    /// Generic pool
    /// </summary>
    public interface IPool
    {
        /// <summary>
        /// Gets an item from the pool if one is available, otherwise creates one.
        /// </summary>
        T Get<T>() where T : class, new();

        /// <summary>
        /// Return an item to the pool.
        /// </summary>
        void Return<T>(T value) where T : class, new();
    }

    /// <summary>
    /// Pool
    /// </summary>
    public interface IPool<T> where T : class, new()
    {
        /// <summary>
        /// <inheritdoc cref="IPool.Get{T}"/>
        /// </summary>
        T Get();

        /// <summary>
        /// <inheritdoc cref="IPool.Return{T}(T)"/>
        /// </summary>
        void Return(T value);
    }
}
