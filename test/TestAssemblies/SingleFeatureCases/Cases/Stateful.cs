using Pooling;

namespace SingleFeatureCases.Cases
{
    public abstract class Stateful<T>
    {
        [NonPooled]
        public static PoolingResult? PoolingResult { get; set; }

        protected static void AvoidOptimize(params object[] items) { }
    }
}
