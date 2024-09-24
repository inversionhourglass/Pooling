using Fody;
using Mono.Cecil;
using System.Collections.Generic;

namespace Pooling.Fody
{
    internal class ResetFuncManager(BaseModuleWeaver moduleWeaver, TypeReference boolTypeRef)
    {
        private readonly Dictionary<MethodDefinition, ResetFunc> _map = [];
        private readonly Dictionary<TypeDefinition, int> _coutingMap = [];
        private TypeDefinition? _resetTypeDef;

        public TypeReference BoolTypeRef { get; } = boolTypeRef;

        public ResetFunc Create(MethodDefinition? resetMethodDef = null)
        {
            if (resetMethodDef == null) return ResetFunc.Absent;

            if (!_map.TryGetValue(resetMethodDef, out var resetFunc))
            {
                var declaringTypeDef = resetMethodDef.DeclaringType;
                if (!_coutingMap.TryGetValue(declaringTypeDef, out var count))
                {
                    count = 0;
                }
                _coutingMap[declaringTypeDef] = count + 1;
                var suffix = count == 0 ? string.Empty : count.ToString();
                resetFunc = new(moduleWeaver.Import(resetMethodDef), suffix, this);
                _map[resetMethodDef] = resetFunc;
            }

            return resetFunc;
        }

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
                moduleWeaver.ModuleDefinition.Types.Add(_resetTypeDef);
            }

            return _resetTypeDef;
        }
    }
}
