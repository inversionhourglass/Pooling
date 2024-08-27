using System;

namespace Pooling
{
    /// <summary>
    /// Pooling exception
    /// </summary>
    public class PoolingException : Exception
    {
        /// <summary>
        /// </summary>
        public PoolingException(string message) : base(message) { }
    }
}
