using System;

namespace SingleFeatureCases.PoolItems
{
    public class IncorrectResetStateException(bool expectedState, Type stateType) : Exception($"[{stateType.Name}] The expected reset state is {expectedState}, but it is actually {!expectedState}")
    {
    }
}
