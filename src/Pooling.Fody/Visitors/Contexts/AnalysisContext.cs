using Cecil.AspectN;
using Cecil.AspectN.Matchers;

namespace Pooling.Fody.Visitors.Contexts
{
    internal abstract class AnalysisContext(MethodSignature methodSignature, ITypeMatcher[]? assemblyNonPooledMatcher, ITypeMatcher[] typeNonPooledMatcher, ITypeMatcher[] methodNonPooledMatcher, Config.Item[] items)
    {
        public MethodSignature MethodSignature => methodSignature;

        public ITypeMatcher[]? AssemblyNonPooledMatcher => assemblyNonPooledMatcher;

        public ITypeMatcher[] TypeNonPooledMatcher => typeNonPooledMatcher;

        public ITypeMatcher[] MethodNonPooledMatcher => methodNonPooledMatcher;

        public Config.Item[] Items => items;
    }
}
