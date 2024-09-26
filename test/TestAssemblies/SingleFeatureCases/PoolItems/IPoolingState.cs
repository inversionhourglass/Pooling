using System;

namespace SingleFeatureCases.PoolItems
{
    public interface IPoolingState
    {
        PoolingState State { get; set; }
    }

    [Flags]
    public enum PoolingState
    {
        None = 0,
        Got = 0x1,
        Returned = 0x2,
        Done = Got | Returned
    }
}
