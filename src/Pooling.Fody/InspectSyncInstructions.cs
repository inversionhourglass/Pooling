using Cecil.AspectN;
using Cecil.AspectN.Matchers;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System.Collections.Generic;
using System.Linq;

namespace Pooling.Fody
{
    partial class ModuleWeaver
    {
        private void InspectSyncInstructions(MethodSignature methodSignature, ITypeMatcher[] typeNonPooledMatcher, ITypeMatcher[] methodNonPooledMatcher, Config.Item[] items)
        {
            var methodDef = methodSignature.Definition;
            var poolItems = SyncAnalysisPoolItems(methodSignature, typeNonPooledMatcher, methodNonPooledMatcher, items);

            if (poolItems.Length == 0) return;

            Pooling(poolItems, false);

            var handler = methodDef.GetOrBuildOutermostExceptionHandler(ExceptionHandlerType.Finally);

            PoolingRecycle(methodDef, handler, poolItems);

            methodDef.Body.InitLocals = true;
            methodDef.Body.OptimizePlus();
        }

        private PoolItem[] SyncAnalysisPoolItems(MethodSignature methodSignature, ITypeMatcher[] typeNonPooledMatcher, ITypeMatcher[] methodNonPooledMatcher, Config.Item[] items)
        {
            var counting = new StackCounting();
            var poolItems = new List<PoolItem>();
            var methodDef = methodSignature.Definition;
            var filterCatches = new List<Instruction>();
            var instruction = methodDef.GetFirstInstruction();

            foreach (var handler in methodDef.Body.ExceptionHandlers)
            {
                if (handler.HandlerType == ExceptionHandlerType.Catch)
                {
                    filterCatches.Add(handler.HandlerStart);
                }
                else if (handler.HandlerType == ExceptionHandlerType.Filter)
                {
                    filterCatches.Add(handler.FilterStart);
                    filterCatches.Add(handler.HandlerStart);
                }
            }

            while (instruction != null)
            {
                if (filterCatches.Contains(instruction))
                {
                    counting.Increase();
                }

                var code = instruction.OpCode.Code;
                switch (code)
                {
                    case Code.Jmp:
                        return [];
                    case Code.Ret:
                        if (!methodDef.ReturnType.IsVoid()) counting.Decrease();
                        if (counting.StackNotEmpty)
                        {
                            throw new FodyWeavingException($"Failed analysis the instructions, There still has a value on the stack after ret(offset: {instruction.Offset}).");
                        }
                        break;
                    case Code.No:
                        throw new FodyWeavingException($"Please share your assembly with me; there is an instruction 'no.' that I have never encountered before, offset: {instruction.Offset}.");
                    case Code.Newobj:
                        counting.Increase();
                        var detectedPoolItem = InspectInstruction(methodSignature, instruction, typeNonPooledMatcher, methodNonPooledMatcher, items);
                        if (detectedPoolItem != null)
                        {
                            counting.Add(detectedPoolItem);
                        }
                        break;
                    case Code.Stloc_0:
                    case Code.Stloc_1:
                    case Code.Stloc_2:
                    case Code.Stloc_3:
                    case Code.Stloc_S:
                    case Code.Stloc:
                        var allocatingPoolItem = counting.Decrease();
                        if (allocatingPoolItem != null)
                        {
                            allocatingPoolItem.Storing = instruction;
                            poolItems.Add(allocatingPoolItem);
                        }
                        break;
                    case Code.Stfld:
                        VariablePersistentCheck(methodDef, poolItems, instruction.Previous);
                        counting.Decrease(2);
                        break;
                    case Code.Stsfld:
                        VariablePersistentCheck(methodDef, poolItems, instruction.Previous);
                        counting.Decrease();
                        break;
                    case Code.Call:
                    case Code.Callvirt:
                        var mr = (MethodReference)instruction.Operand;
                        var md = mr.ToDefinition();

                        if (md.IsSetter) VariablePersistentCheck(methodDef, poolItems, instruction.Previous);

                        var staticAmount = md.IsStatic ? 0 : 1;
                        var amount = mr.Parameters.Count + staticAmount;
                        counting.Decrease(amount);
                        if (!mr.ReturnType.IsVoid())
                        {
                            counting.Increase();  // increase和decrease是有额外逻辑处理的，最好不要合并
                        }
                        break;
                    case Code.Calli:   // 不常见用法，通过指针调用方法
                        var callSite = (CallSite)instruction.Operand;
                        counting.Decrease(callSite.Parameters.Count + 1);
                        if (!callSite.ReturnType.IsVoid())
                        {
                            counting.Increase();  // increase和decrease是有额外逻辑处理的，最好不要合并
                        }
                        break;
                    case Code.Ldarg:
                    case Code.Ldarg_S:
                    case Code.Ldarg_0:
                    case Code.Ldarg_1:
                    case Code.Ldarg_2:
                    case Code.Ldarg_3:
                    case Code.Ldarga:
                    case Code.Ldarga_S:
                    case Code.Ldloca:
                    case Code.Ldloca_S:
                    case Code.Ldloc:
                    case Code.Ldloc_S:
                    case Code.Ldloc_0:
                    case Code.Ldloc_1:
                    case Code.Ldloc_2:
                    case Code.Ldloc_3:
                    case Code.Ldc_I4_M1:
                    case Code.Ldc_I4_0:
                    case Code.Ldc_I4_1:
                    case Code.Ldc_I4_2:
                    case Code.Ldc_I4_3:
                    case Code.Ldc_I4_4:
                    case Code.Ldc_I4_5:
                    case Code.Ldc_I4_6:
                    case Code.Ldc_I4_7:
                    case Code.Ldc_I4_8:
                    case Code.Ldc_I4_S:
                    case Code.Ldc_I4:
                    case Code.Ldc_I8:
                    case Code.Ldc_R4:
                    case Code.Ldc_R8:
                    case Code.Dup:
                    case Code.Ldnull:
                    case Code.Ldstr:
                    case Code.Ldsfld:
                    case Code.Ldsflda:
                    case Code.Ldtoken:
                    case Code.Ldftn:
                    case Code.Arglist:
                    case Code.Ldvirtftn:
                    case Code.Sizeof:
                        counting.Increase();
                        break;
                    case Code.Stelem_I:
                    case Code.Stelem_I1:
                    case Code.Stelem_I2:
                    case Code.Stelem_I4:
                    case Code.Stelem_I8:
                    case Code.Stelem_R4:
                    case Code.Stelem_R8:
                    case Code.Stelem_Ref:
                    case Code.Stelem_Any:
                    case Code.Cpblk:
                    case Code.Initblk:
                        counting.Decrease(3);
                        break;
                    case Code.Beq_S:
                    case Code.Bge_S:
                    case Code.Bgt_S:
                    case Code.Ble_S:
                    case Code.Blt_S:
                    case Code.Bne_Un_S:
                    case Code.Bge_Un_S:
                    case Code.Bgt_Un_S:
                    case Code.Ble_Un_S:
                    case Code.Blt_Un_S:
                    case Code.Beq:
                    case Code.Bge:
                    case Code.Bgt:
                    case Code.Ble:
                    case Code.Blt:
                    case Code.Bne_Un:
                    case Code.Bge_Un:
                    case Code.Bgt_Un:
                    case Code.Ble_Un:
                    case Code.Blt_Un:
                    case Code.Cpobj:
                    case Code.Stobj:
                    case Code.Stind_I:
                    case Code.Stind_I1:
                    case Code.Stind_I2:
                    case Code.Stind_I4:
                    case Code.Stind_I8:
                    case Code.Stind_R4:
                    case Code.Stind_R8:
                    case Code.Stind_Ref:
                        counting.Decrease(2);
                        break;
                    case Code.Add:
                    case Code.Sub:
                    case Code.Mul:
                    case Code.Div:
                    case Code.Div_Un:
                    case Code.Rem:
                    case Code.Rem_Un:
                    case Code.Add_Ovf:
                    case Code.Add_Ovf_Un:
                    case Code.Mul_Ovf:
                    case Code.Mul_Ovf_Un:
                    case Code.Sub_Ovf:
                    case Code.Sub_Ovf_Un:
                    case Code.And:
                    case Code.Or:
                    case Code.Xor:
                    case Code.Shl:
                    case Code.Shr:
                    case Code.Shr_Un:
                    case Code.Ceq:
                    case Code.Cgt:
                    case Code.Cgt_Un:
                    case Code.Clt:
                    case Code.Clt_Un:
                    case Code.Ldelem_I1:
                    case Code.Ldelem_U1:
                    case Code.Ldelem_I2:
                    case Code.Ldelem_U2:
                    case Code.Ldelem_I4:
                    case Code.Ldelem_U4:
                    case Code.Ldelem_I8:
                    case Code.Ldelem_I:
                    case Code.Ldelem_R4:
                    case Code.Ldelem_R8:
                    case Code.Ldelem_Ref:
                    case Code.Ldelem_Any:
                    case Code.Ldelema:
                    case Code.Starg:
                    case Code.Starg_S:
                    case Code.Initobj:
                    case Code.Pop:
                    case Code.Throw:
                    case Code.Switch:
                    case Code.Brfalse_S:
                    case Code.Brtrue_S:
                    case Code.Brfalse:
                    case Code.Brtrue:
                    case Code.Endfilter:
                        counting.Decrease();
                        break;
                    case Code.Br_S:
                    case Code.Br:
                        break;
                    case Code.Leave:
                    case Code.Leave_S:
                    case Code.Endfinally:
                        if (counting.StackNotEmpty)
                        {
                            throw new FodyWeavingException($"Failed analysis the instructions, There still has a value on the stack after {code}(offset: {instruction.Offset}).");
                        }
                        break;
                    // 以下指令是对栈上的值进行操作，不会改变栈的深度
                    case Code.Nop:
                    case Code.Break:
                    case Code.Readonly:
                    case Code.Neg:
                    case Code.Not:
                    case Code.Conv_I:
                    case Code.Conv_I1:
                    case Code.Conv_I2:
                    case Code.Conv_I4:
                    case Code.Conv_I8:
                    case Code.Conv_R4:
                    case Code.Conv_R8:
                    case Code.Conv_U:
                    case Code.Conv_U1:
                    case Code.Conv_U2:
                    case Code.Conv_U4:
                    case Code.Conv_U8:
                    case Code.Conv_R_Un:
                    case Code.Conv_Ovf_I:
                    case Code.Conv_Ovf_I1:
                    case Code.Conv_Ovf_I2:
                    case Code.Conv_Ovf_I4:
                    case Code.Conv_Ovf_I8:
                    case Code.Conv_Ovf_I_Un:
                    case Code.Conv_Ovf_I1_Un:
                    case Code.Conv_Ovf_I2_Un:
                    case Code.Conv_Ovf_I4_Un:
                    case Code.Conv_Ovf_I8_Un:
                    case Code.Conv_Ovf_U:
                    case Code.Conv_Ovf_U1:
                    case Code.Conv_Ovf_U2:
                    case Code.Conv_Ovf_U4:
                    case Code.Conv_Ovf_U8:
                    case Code.Conv_Ovf_U_Un:
                    case Code.Conv_Ovf_U1_Un:
                    case Code.Conv_Ovf_U2_Un:
                    case Code.Conv_Ovf_U4_Un:
                    case Code.Conv_Ovf_U8_Un:
                    case Code.Ldind_I1:
                    case Code.Ldind_U1:
                    case Code.Ldind_I2:
                    case Code.Ldind_U2:
                    case Code.Ldind_I4:
                    case Code.Ldind_U4:
                    case Code.Ldind_I8:
                    case Code.Ldind_I:
                    case Code.Ldind_R4:
                    case Code.Ldind_R8:
                    case Code.Ldind_Ref:
                    case Code.Castclass:
                    case Code.Isinst:
                    case Code.Unbox:
                    case Code.Unbox_Any:
                    case Code.Box:
                    case Code.Ldobj:
                    case Code.Ldfld:
                    case Code.Ldflda:
                    case Code.Ldlen:
                    case Code.Newarr:
                    case Code.Refanyval:
                    case Code.Ckfinite:
                    case Code.Mkrefany:
                    case Code.Refanytype:
                    case Code.Localloc:
                    case Code.Unaligned:
                    case Code.Volatile:
                    case Code.Tail:
                    case Code.Constrained:
                    case Code.Rethrow:
                        break;
                    default:
                        throw new FodyWeavingException($"Unrecognized opcode {code}, offset: {instruction.Offset}");
                }
                instruction = instruction.Next;
            }

            return poolItems.ToArray();
        }

        private PoolItem? InspectInstruction(MethodSignature methodSignature, Instruction newObj, ITypeMatcher[] typeNonPooledMatcher, ITypeMatcher[] methodNonPooledMatcher, Config.Item[] poolItems)
        {
            if (newObj.Operand is not MethodReference ctor || ctor.Parameters.Count != 0) return null;

            var methodDef = methodSignature.Definition;
            var typeRef = ctor.DeclaringType;
            var typeSignature = SignatureParser.ParseType(typeRef);

            if (_assemblyNonPooledMatcher != null && _assemblyNonPooledMatcher.Any(x => x.IsMatch(typeSignature)))
            {
                WriteDebug($"[{methodDef}] The type {typeSignature.Reference} is marked as NonPooledAttribute in the assembly level, so the new operation will not be replaced by the pool operation.");
                return null;
            }
            if (typeNonPooledMatcher.Any(x => x.IsMatch(typeSignature)))
            {
                WriteDebug($"[{methodDef}] The type {typeSignature.Reference} is marked as NonPooledAttribute in the type level, so the new operation will not be replaced by the pool operation.");
                return null;
            }
            if (methodNonPooledMatcher.Any(x => x.IsMatch(typeSignature)))
            {
                WriteDebug($"[{methodDef}] The type {typeSignature.Reference} is marked as NonPooledAttribute in the method level, so the new operation will not be replaced by the pool operation.");
                return null;
            }

            if (typeRef.Implement(Constants.TYPE_IPoolItem))
            {
                var exclusive = PoolItemExclusive.Resolve(typeRef);
                return exclusive.IsExcluded(methodSignature) ? null : new(newObj, typeRef);
            }

            ITypeMatcher? stateless = null;
            foreach (var item in poolItems)
            {
                if (item.Pattern != null)
                {
                    if (!item.Pattern.SupportDeclaringTypeMatch || item.Pattern.DeclaringTypeMatcher.IsMatch(typeSignature))
                    {
                        var resetMethodDef = typeSignature.FindMethod(item.Pattern, _config.CompositeAccessibility);
                        if (resetMethodDef != null)
                        {
                            if (!resetMethodDef.IsAbstract && !resetMethodDef.IsStatic &&
                                !resetMethodDef.HasPInvokeInfo && !resetMethodDef.IsPInvokeImpl &&
                                resetMethodDef.GenericParameters.Count == 0 && resetMethodDef.Parameters.Count == 0)
                            {
                                return new(newObj, typeRef, resetMethodDef);
                            }
                        }
                    }
                }
                else if (stateless == null && item.Stateless != null && item.Stateless.IsMatch(typeSignature))
                {
                    stateless = item.Stateless;
                }
            }

            return stateless == null ? null : new(newObj, typeRef);
        }

        private void Pooling(PoolItem[] poolItems, bool maybeField)
        {
            foreach (var poolItem in poolItems)
            {
                var trPool = _trPool.MakeGenericInstanceType(this.Import(poolItem.ItemTypeRef));
                var mrGet = _mrGet.WithGenericDeclaringType(trPool);
                poolItem.NewObj.Set(OpCodes.Call, mrGet);

                if (poolItem.Storing == null) throw new FodyWeavingException("Failed to analyze the instructions; the instruction for storing the variable was not found.");

                if (maybeField && poolItem.Storing.IsStfld())
                {
                    poolItem.Loading = new([Instruction.Create(OpCodes.Ldarg_0), poolItem.Storing.Stfld2Ldfld()]);
                }
                else
                {
                    poolItem.Loading = new(poolItem.Storing.Stloc2Ldloc());
                }
            }
        }

        private void PoolingRecycle(MethodDefinition methodDef, ExceptionHandler handler, PoolItem[] poolItems)
        {
            var instructions = methodDef.Body.Instructions;
            var endFinallyOrLeave = handler.HandlerEnd.Previous;
            Instruction? nextPoolItemRecycleStart = null;
            foreach (var poolItem in poolItems)
            {
                var trPool = _trPool.MakeGenericInstanceType(this.Import(poolItem.ItemTypeRef));
                var mrReturn = _mrReturn.WithGenericDeclaringType(trPool);

                var genericArguments = poolItem.ItemTypeRef is GenericInstanceType git ? git.GenericArguments.ToArray() : [];
                var blockStart = nextPoolItemRecycleStart == null ?
                                    poolItem.Loading!.Clone() :
                                    nextPoolItemRecycleStart.SetFirstToCloned(poolItem.Loading!);
                nextPoolItemRecycleStart = Instruction.Create(OpCodes.Nop);
                instructions.InsertBefore(endFinallyOrLeave, [
                    .. blockStart,
                    Instruction.Create(OpCodes.Brfalse, nextPoolItemRecycleStart),
                    .. poolItem.CallResetMethod(this, genericArguments),
                    .. poolItem.Loading,
                    Instruction.Create(OpCodes.Call, mrReturn)
                ]);
            }

            if (nextPoolItemRecycleStart != null)
            {
                instructions.InsertBefore(endFinallyOrLeave, nextPoolItemRecycleStart);
            }
        }

        private static bool VariablePersistentCheck(MethodDefinition methodDef, List<PoolItem> poolItems, Instruction previous)
        {
            if (!previous.IsLdloc()) return false;

            if (previous.TryResolveVariable(methodDef, out var variable))
            {
                poolItems.RemoveAll(x => x.Storing != null && x.Storing.TryResolveVariable(methodDef, out var v) && v == variable);
            }

            return true;
        }
    }
}
