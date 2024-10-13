using Cecil.AspectN;
using Cecil.AspectN.Matchers;

namespace Pooling.Fody.Visitors.Contexts
{
    internal class AnalysisSyncMethodContext(MethodSignature methodSignature, ITypeMatcher[]? assemblyNonPooledMatcher, ITypeMatcher[] typeNonPooledMatcher, ITypeMatcher[] methodNonPooledMatcher, Config.Item[] items) : AnalysisContext(methodSignature, assemblyNonPooledMatcher, typeNonPooledMatcher, methodNonPooledMatcher, items)
    {
    }
}
