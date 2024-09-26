using SingleFeatureCases.PoolItems;
using Pooling;
using System.Collections.Generic;
using System.Linq;

namespace SingleFeatureCases
{
    [NonPooled]
    public class PoolingResult
    {
        private readonly List<IPoolingState> _expectedPooled = [];
        private readonly List<IPoolingState> _expectedNotPooled = [];

        private PoolingResult() { }

        /// <summary>
        /// 期望没有进行任何池化步骤但却进行了部分或完整的池化步骤的类型
        /// </summary>
        public string[] UnexpectedPooled => _expectedNotPooled.Where(x => x.State != PoolingState.None).Select(x => $"[{x.State}] {x.GetType().Name}").ToArray();

        /// <summary>
        /// 期望完成整个池化步骤但却未能完成的类型
        /// </summary>
        public string[] UnexpectedNotPooled => _expectedPooled.Where(x => x.State != PoolingState.Done).Select(x => $"[{x.State}] {x.GetType().Name}").ToArray();

        public void ShouldPooled(params IPoolingState[] resetStates)
        {
            foreach (var state in resetStates)
            {
                ShouldPooled(state);
            }
        }

        public void ShouldPooled(IPoolingState resetState)
        {
            _expectedPooled.Add(resetState);
        }

        public void ShouldNotPooled(params IPoolingState[] resetStates)
        {
            foreach(var state in resetStates)
            {
                ShouldNotPooled(state);
            }
        }
        public void ShouldNotPooled(IPoolingState resetState)
        {
            _expectedNotPooled.Add(resetState);
        }

        public static PoolingResult New() => new();
    }
}
