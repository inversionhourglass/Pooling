using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Pooling.Fody
{
    internal class PoolItem(Instruction newObj, TypeReference itemTypeRef, MethodDefinition? resetMethodDef = null)
    {
        public Instruction NewObj { get; } = newObj;

        public Instruction? Storing { get; set; }

        public Instructions? Loading { get; set; }

        public TypeReference ItemTypeRef { get; } = itemTypeRef;

        public MethodDefinition? ResetMethodDef { get; } = resetMethodDef;

        public Instruction[] CallResetMethod(BaseModuleWeaver moduleWeaver, TypeReference[] genericArguments)
        {
            if (ResetMethodDef == null) return [];

            List<Instruction> calling = [
                .. Loading!.Clone(),
                moduleWeaver.Import(ResetMethodDef).WithGenerics(genericArguments).CallAny()
            ];
            if (!ResetMethodDef.ReturnType.IsVoid())
            {
                calling.Add(Instruction.Create(OpCodes.Pop));
            }

            return calling.ToArray();
        }
    }
}
