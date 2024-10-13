using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SingleFeatureCases.Cases.ILInspects
{
    internal class GenericTypeMethod<T>
    {
        private async ValueTask<object?> LastInner<TT>(List<T> list1, List<TT> list2)
        {
            await Task.Yield();
            return list1.Count > list2.Count ? list1.Last() : list2.Last();
        }
    }
}
