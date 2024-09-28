using Cecil.AspectN;
using Cecil.AspectN.Matchers;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pooling.Fody
{
    partial class ModuleWeaver
    {
        private void InspectStateMachineInstructions(MethodSignature methodSignature, TypeDefinition tdStateMachine, CustomAttribute stateMachineAttribute, ITypeMatcher[] typeNonPooledMatcher, ITypeMatcher[] methodNonPooledMatcher, Config.Item[] items)
        {
            var mdMoveNext = tdStateMachine.GetMethod(Constants.METHOD_MoveNext, false);
            ExceptionHandler? stateMachineCatching = null;
            if (!stateMachineAttribute.Is(Constants.TYPE_IteratorStateMachineAttribute))
            {
                stateMachineCatching = mdMoveNext.GetOuterExceptionHandler();
                if (stateMachineCatching == null || stateMachineCatching.HandlerType != ExceptionHandlerType.Catch) throw new FodyWeavingException($"[{mdMoveNext}] Cannot get the correct outermost catch handler that Microsoft generated for the state machine type; maybe the MSIL has been modified.");
            }

            var poolItems = StateMachineAnalysisPoolItems(methodSignature, tdStateMachine, mdMoveNext, stateMachineCatching, typeNonPooledMatcher, methodNonPooledMatcher, items);

            if (poolItems.Length == 0) return;

            Pooling(poolItems, true);

            if (stateMachineCatching == null)
            {
                var handler = mdMoveNext.GetOrBuildOutermostExceptionHandler(ExceptionHandlerType.Finally);
                PoolingRecycle(mdMoveNext, handler, poolItems);
            }
            else if (TryResolveStateFieldAndVariable(tdStateMachine, mdMoveNext, stateMachineCatching, out var fdState, out var vState) &&
                OfficalStateUsage(mdMoveNext, stateMachineCatching, fdState, vState!) == false)
            {
                // 非官方格式就用比较强硬的方式处理
                throw new NotImplementedException("eeeeeeeeeeeeeeeeeeeeeeeeeeeee");
            }
            else
            {
                var handler = StateMachineBuildTryFinally(mdMoveNext, stateMachineCatching, vState);
                PoolingRecycle(mdMoveNext, handler, poolItems);
            }

            mdMoveNext.Body.InitLocals = true;
            mdMoveNext.Body.OptimizePlus();

            // 1. 找到state变量
            // 2. 查找是否存在标准的state赋值-1的代码
            // 2.0. 不存在state变量
            //   -> 使用了async语法，但方法实际没有任何await操作，变量都不存在，一般是release模式
            // 2.1. 不存在操作state变量的代码
            //   -> 使用了async语法，但方法实际没有任何await操作，按2.2的方式处理
            // 2.2. 存在操作state变量的代码且存在标准的state赋值-1的代码
            //   -> C#编译器生成的标准代码，直接try..finally..判断state是否为-1，-1表示需要将对象返回池
            // 2.3. 存在操作state变量的代码但不存在标准的state赋值-1的代码
            //   -> 某种自定义生成的StateMachine（比如Rougamo），直接在外层try..catch..中处理，不再讲究StateMachine代码格式，主打一个能用就行
        }

        private PoolItem[] StateMachineAnalysisPoolItems(MethodSignature methodSignature, TypeDefinition tdStateMachine, MethodDefinition mdMoveNext, ExceptionHandler? stateMachineCatching, ITypeMatcher[] typeNonPooledMatcher, ITypeMatcher[] methodNonPooledMatcher, Config.Item[] items)
        {
            var counting = new StackCounting();
            var poolItems = new List<PoolItem>();

            var instruction = stateMachineCatching == null ? mdMoveNext.Body.Instructions.FirstOrDefault() : stateMachineCatching.TryStart;
            var endInstruction = stateMachineCatching?.TryEnd;

            while (instruction != null && instruction != endInstruction)
            {
                var code = instruction.OpCode.Code;
                switch (code)
                {
                    case Code.Jmp:
                        return [];
                    case Code.Ret:
                        if (!mdMoveNext.ReturnType.IsVoid()) counting.Decrease();
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
                        var variableAllocatingPoolItem = counting.Decrease();
                        if (variableAllocatingPoolItem != null)
                        {
                            variableAllocatingPoolItem.Storing = instruction;
                            poolItems.Add(variableAllocatingPoolItem);
                        }
                        break;
                    case Code.Stfld:
                        var fr = (FieldReference)instruction.Operand;
                        var fieldAllocatingPoolItem = counting.Decrease(2);
                        if (fieldAllocatingPoolItem != null)
                        {
                            if (fr.DeclaringType.ToDefinition() == tdStateMachine)
                            {
                                fieldAllocatingPoolItem.Storing = instruction;
                                poolItems.Add(fieldAllocatingPoolItem);
                                break;
                            }
                        }
                        StateMachinePersistentCheck(mdMoveNext, poolItems, instruction.Previous);
                        break;
                    case Code.Stsfld:
                        StateMachinePersistentCheck(mdMoveNext, poolItems, instruction.Previous);
                        counting.Decrease();
                        break;
                    case Code.Call:
                    case Code.Callvirt:
                        var mr = (MethodReference)instruction.Operand;
                        var md = mr.ToDefinition();

                        if (md.IsSetter) StateMachinePersistentCheck(mdMoveNext, poolItems, instruction.Previous);

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
                        counting.Decrease();
                        break;
                    case Code.Br_S:
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
                    case Code.Br:
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
                        break;
                    case Code.Leave:
                    case Code.Leave_S:
                    case Code.Endfinally:
                    case Code.Endfilter:
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

        private bool TryResolveStateFieldAndVariable(TypeDefinition tdStateMachine, MethodDefinition mdMoveNext, ExceptionHandler stateMachineCatching, out FieldDefinition fdState, out VariableDefinition? vState)
        {
            fdState = tdStateMachine.Fields.Single(x => x.Name == Constants.FIELD_State);

            if (!mdMoveNext.Body.Variables.Any(x => x.VariableType.IsInt32()))
            {
                vState = null;
                return false;
            }

            var instruction = mdMoveNext.Body.Instructions.First();

            while (instruction != null && instruction != stateMachineCatching.TryEnd)
            {
                if (instruction.OpCode.Code == Code.Ldfld && instruction.Operand is FieldReference fr && fr.ToDefinition()  == fdState)
                {
                    var next = instruction.Next;
                    if (next.IsStloc() && next.TryResolveVariable(mdMoveNext, out vState))
                    {
                        return true;
                    }
                }
                instruction = instruction.Next;
            }

            vState = null;
            return false;
        }

        /// <summary>
        ///  是否按官方格式使用了state字段及变量
        /// </summary>
        /// <returns>
        /// true: 官方格式<br/>
        /// false: 非官方格式<br/>
        /// null: 没有使用state变量及字段，一般debug模式下对于使用了async语法但实际没有await操作的方法会出现这种情况
        /// </returns>
        /// <remarks>
        /// 官方格式是在每次yield后的continue时将state字段及变量设置为-1，标志性的MSIL为：
        /// <![CDATA[
        /// ldarg.0
        /// ldc.i4.m1
        /// dup
        /// stloc.0    // 如果索引为0的变量不是是state变量，这里也可能是其他stloc操作
        /// stfld int32 '<>1__state'
        /// ]]>
        /// </remarks>
        private bool? OfficalStateUsage(MethodDefinition mdMoveNext, ExceptionHandler stateMachineCatching, FieldDefinition fdState, VariableDefinition vState)
        {
            var stateUsed = false;
            var instruction = stateMachineCatching.TryStart;

            while (instruction != null && instruction != stateMachineCatching.TryEnd)
            {
                if (instruction.IsStfld() && instruction.Operand is FieldReference fr && fr.ToDefinition() == fdState)
                {
                    if (instruction.Previous.IsStloc() && instruction.Previous.TryResolveVariable(mdMoveNext, out var v) && v == vState &&
                        instruction.Previous.Previous.IsDup() &&
                        instruction.Previous.Previous.Previous.IsLdcM1() &&
                        instruction.Previous.Previous.Previous.Previous.IsLdarg0())
                    {
                        return true;
                    }
                    stateUsed = true;
                }
                instruction = instruction.Next;
            }

            return stateUsed ? false : null;
        }

        private ExceptionHandler StateMachineBuildTryFinally(MethodDefinition mdMoveNext, ExceptionHandler stateMachineCatching, VariableDefinition? vState)
        {
            var tryStart = stateMachineCatching.TryStart;
            var tryEnd = Instruction.Create(OpCodes.Nop);
            var finallyEnd = stateMachineCatching.HandlerStart;
            var endFinally = Instruction.Create(OpCodes.Endfinally);

            var instructions = mdMoveNext.Body.Instructions;

            instructions.InsertBefore(finallyEnd, tryEnd);
            if (vState != null)
            {
                instructions.InsertBefore(finallyEnd, [
                    Instruction.Create(OpCodes.Ldloc, vState),
                    Instruction.Create(OpCodes.Ldc_I4_0),
                    Instruction.Create(OpCodes.Bge, endFinally),
                ]);
            }
            instructions.InsertBefore(finallyEnd, endFinally);

            var handler = new ExceptionHandler(ExceptionHandlerType.Finally)
            {
                TryStart = tryStart,
                TryEnd = tryEnd,
                HandlerStart = tryEnd,
                HandlerEnd = finallyEnd
            };
            mdMoveNext.Body.ExceptionHandlers.Insert(0, handler);

            return handler;
        }

        private static void StateMachinePersistentCheck(MethodDefinition methodDef, List<PoolItem> poolItems, Instruction previous)
        {
            if (VariablePersistentCheck(methodDef, poolItems, previous)) return;

            FieldPersistentCheck(methodDef, poolItems, previous);
        }

        private static bool FieldPersistentCheck(MethodDefinition methodDef, List<PoolItem> poolItems, Instruction previous)
        {
            if (previous.IsLdfld()) return false;

            if (previous.TryResolveVariable(methodDef, out var variable))
            {
                poolItems.RemoveAll(x => x.Storing != null && x.Storing.TryResolveVariable(methodDef, out var v) && v == variable);
            }

            return true;
        }
    }
}
