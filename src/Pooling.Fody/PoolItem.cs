using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;

namespace Pooling.Fody
{
    internal class PoolItem(Instruction newObj, TypeReference itemTypeRef, MethodDefinition? resetMethodDef = null) : IEquatable<PoolItem>
    {
        public Instruction NewObj { get; } = newObj;

        public Instruction? Storing { get; set; }

        public Instructions? Loading { get; set; }

        public TypeReference ItemTypeRef { get; } = itemTypeRef;

        public MethodDefinition? ResetMethodDef { get; } = resetMethodDef;

        public Instruction[] CallResetMethod(BaseModuleWeaver moduleWeaver, TypeReference[] genericArguments, Instruction ifResetFailedTo)
        {
            if (ResetMethodDef == null) return [];

            List<Instruction> calling = [
                .. Loading!.Clone(),
                moduleWeaver.Import(ResetMethodDef).WithGenerics(genericArguments).CallAny()
            ];
            if (ResetMethodDef.ReturnType.IsBool())
            {
                calling.Add(Instruction.Create(OpCodes.Brfalse, ifResetFailedTo));
            }
            else if (!ResetMethodDef.ReturnType.IsVoid())
            {
                calling.Add(Instruction.Create(OpCodes.Pop));
            }

            return calling.ToArray();
        }

        public bool Equals(PoolItem? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return NewObj.Equals(other.NewObj) && Equals(Storing, other.Storing);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as PoolItem);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + NewObj.GetHashCode();
                if (Storing != null)
                {
                    hash = hash * 23 + Storing.GetHashCode();
                }
                return hash;
            }
        }
    }
}
