using SingleFeatureCases.PoolItems;
using Pooling;

namespace SingleFeatureCases
{
    public class StatefulPool : IPool
    {
        public T Get<T>() where T : class, new()
        {
            var value = new T();
            if (value is IPoolingState poolingState)
            {
                poolingState.State |= PoolingState.Got;
            }

            return value;
        }

        public void Return<T>(T value) where T : class, new()
        {
            if (value is IPoolingState poolingState)
            {
                poolingState.State |= PoolingState.Returned;
            }
        }
    }
}
