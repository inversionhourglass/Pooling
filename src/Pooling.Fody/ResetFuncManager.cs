using Mono.Cecil;

namespace Pooling.Fody
{
    internal class ResetFuncManager(ModuleDefinition moduleDef, TypeReference boolTypeRef)
    {
        private TypeDefinition? _resetTypeDef;

        public TypeReference BoolTypeRef { get; } = boolTypeRef;

        public ResetFunc Create(MethodDefinition? resetMethodDef = null) => new(resetMethodDef, this);

        public void Add(MethodDefinition tryResetMethodDef)
        {
            var resetTypeDef = GetResetType();

            resetTypeDef.Methods.Add(tryResetMethodDef);
        }

        private TypeDefinition GetResetType()
        {
            if (_resetTypeDef == null)
            {
                var typeAttribute = TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit;
                _resetTypeDef = new TypeDefinition(string.Empty, ">_Pooling_Reset", typeAttribute);
                moduleDef.Types.Add(_resetTypeDef);
            }

            return _resetTypeDef;
        }
    }
}
