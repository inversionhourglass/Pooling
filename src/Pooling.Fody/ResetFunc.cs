using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Linq;

namespace Pooling.Fody
{
    internal class ResetFunc(MethodDefinition? resetMethodDef, string suffix, ResetFuncManager manager)
    {
        private MethodDefinition? _tryResetMethodDef;

        public static ResetFunc Absent { get; } = new(null, string.Empty, null!);

        public bool Has => resetMethodDef != null;

        public Instruction Load(TypeReference[] genericParameters)
        {
            if (resetMethodDef == null) return Instruction.Create(OpCodes.Ldnull);

            if (_tryResetMethodDef == null)
            {
                _tryResetMethodDef = BuildTryResetMethod("TryReset" + suffix, resetMethodDef, manager.BoolTypeRef);
                manager.Add(_tryResetMethodDef);
            }

            return Instruction.Create(OpCodes.Ldftn, _tryResetMethodDef.WithGenerics(genericParameters));
        }

        private static MethodDefinition BuildTryResetMethod(string methodName, MethodDefinition resetMethodDef, TypeReference returnTypeRef)
        {
            var methodAttributes = MethodAttributes.Assembly | MethodAttributes.Static | MethodAttributes.HideBySig;
            var tryResetMethodDef = new MethodDefinition(methodName, methodAttributes, returnTypeRef);
            var typeDef = resetMethodDef.DeclaringType;
            TypeReference typeRef = typeDef;
            //MethodReference methodRef;

            if (typeDef.HasGenericParameters)
            {
                var genericParameters = typeDef.GenericParameters.Select(x => x.Clone(tryResetMethodDef)).ToArray();
                //typeRef = typeDef.MakeGenericInstanceType(genericParameters);
                //methodRef = methodDef.WithGenericDeclaringType(typeRef);

                tryResetMethodDef.GenericParameters.Add(genericParameters);
            }

            var vTarget = new ParameterDefinition("target", ParameterAttributes.None, typeRef);
            tryResetMethodDef.Parameters.Add(vTarget);

            BuildTryResetMethodBody(tryResetMethodDef, vTarget, resetMethodDef);

            return tryResetMethodDef;
        }

        private static void BuildTryResetMethodBody(MethodDefinition tryResetMethodDef, ParameterDefinition vTarget, MethodReference methodRef)
        {
            var instructions = tryResetMethodDef.Body.Instructions;

            instructions.Add(Instruction.Create(OpCodes.Ldarg, vTarget));
            instructions.Add(Instruction.Create(OpCodes.Callvirt, methodRef));
            if (methodRef.ReturnType.IsVoid())
            {
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4_1));
            }
            else if (!methodRef.ReturnType.IsBool())
            {
                instructions.Add(Instruction.Create(OpCodes.Pop));
            }
            instructions.Add(Instruction.Create(OpCodes.Ret));
        }
    }
}
