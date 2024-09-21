using Cecil.AspectN;
using Cecil.AspectN.Patterns;
using System.Collections.Generic;

namespace Pooling.Fody.AspectN.Patterns
{
    internal class ForceResetTypePattern(IIntermediateTypePattern pattern) : IIntermediateTypePattern
    {
        public bool IsAny => pattern.IsAny;

        public bool IsVoid => pattern.IsVoid;

        public bool AssignableMatch => pattern.AssignableMatch;

        public void Compile(List<GenericParameterTypePattern> genericParameters, bool genericIn)
        {
            pattern.Compile(genericParameters, genericIn);
        }

        public bool IsMatch(TypeSignature signature)
        {
            return pattern.IsMatch(signature);
        }

        public GenericNamePattern SeparateOutMethod()
        {
            return pattern.SeparateOutMethod();
        }

        public DeclaringTypeMethodPattern ToDeclaringTypeMethod(params string[] methodImplicitPrefixes)
        {
            return pattern.ToDeclaringTypeMethod(methodImplicitPrefixes);
        }
    }
}
