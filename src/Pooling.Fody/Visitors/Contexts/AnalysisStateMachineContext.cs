using Cecil.AspectN;
using Cecil.AspectN.Matchers;
using Mono.Cecil;

namespace Pooling.Fody.Visitors.Contexts
{
    internal class AnalysisStateMachineContext(MethodSignature methodSignature, TypeDefinition tdStateMachine, MethodDefinition mdMoveNext, ITypeMatcher[]? assemblyNonPooledMatcher, ITypeMatcher[] typeNonPooledMatcher, ITypeMatcher[] methodNonPooledMatcher, Config.Item[] items) : AnalysisContext(methodSignature, assemblyNonPooledMatcher, typeNonPooledMatcher, methodNonPooledMatcher, items)
    {
        public TypeDefinition TdStateMachine => tdStateMachine;

        public MethodDefinition MdMoveNext => mdMoveNext;
    }
}
