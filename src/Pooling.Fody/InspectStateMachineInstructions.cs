using Cecil.AspectN;
using Cecil.AspectN.Matchers;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Pooling.Fody.Visitors;
using Pooling.Fody.Visitors.Contexts;
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
            var context = new AnalysisStateMachineContext(methodSignature, tdStateMachine, mdMoveNext, _assemblyNonPooledMatcher, typeNonPooledMatcher, methodNonPooledMatcher, items);
            var visitor = new StateMachineVisitor(this, context);
            visitor.Visit();

            var poolItems = visitor.PoolItems.Distinct().ToArray();

            if (poolItems.Length == 0) return;

            Pooling(poolItems, true);

            ExceptionHandler handler;
            if (stateMachineCatching == null)
            {
                // IteratorStateMachine的MoveNext没有try..catch..，将其作为同步方法处理
                handler = mdMoveNext.BuildOutermostExceptionHandler(ExceptionHandlerType.Finally);
            }
            else if (TryResolveStateFieldAndVariable(tdStateMachine, mdMoveNext, stateMachineCatching, out var fdState, out var vState) &&
                OfficalStateUsage(mdMoveNext, stateMachineCatching, fdState, vState!) == false)
            {
                // StateMachine非官方格式，比如由Rougamo生成的经过IL优化的StateMachine
                handler = StateMachineUnofficalBuildTryFinally(mdMoveNext, vState!);
            }
            else
            {
                handler = StateMachineBuildTryFinally(mdMoveNext, stateMachineCatching, vState);
            }
            PoolingRecycle(mdMoveNext, handler, poolItems);

            mdMoveNext.Body.InitLocals = true;
            mdMoveNext.Body.OptimizePlus();
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
                if (instruction.OpCode.Code == Code.Ldfld && instruction.Operand is FieldReference fr && fr.ToDefinition() == fdState)
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

            mdMoveNext.UpdateDebugInfoLastSequencePoint();
            mdMoveNext.SkipExceptionHandlerBlockSequencePoint(handler);

            return handler;
        }

        private ExceptionHandler StateMachineUnofficalBuildTryFinally(MethodDefinition mdMoveNext, VariableDefinition vState)
        {
            var instructions = mdMoveNext.Body.Instructions;
            var returnBlock = mdMoveNext.MergeReturnToLeave();
            var tryStart = instructions.First();
            var tryEnd = Instruction.Create(OpCodes.Nop);
            var finallyEnd = returnBlock[0];
            var endFinally = Instruction.Create(OpCodes.Endfinally);
            var lastInstruction = instructions.Last();
            if (lastInstruction.OpCode.Code != Code.Leave && lastInstruction.OpCode.Code != Code.Leave_S || lastInstruction.Operand is not Instruction instruction || instruction != finallyEnd)
            {
                instructions.Add(Instruction.Create(OpCodes.Leave, finallyEnd));
            }

            instructions.Add([
                tryEnd.Set(OpCodes.Ldloc, vState),
                Instruction.Create(OpCodes.Ldc_I4_0),
                Instruction.Create(OpCodes.Bge, endFinally),
                endFinally
            ]);
            instructions.Add(returnBlock);

            var handler = new ExceptionHandler(ExceptionHandlerType.Finally)
            {
                TryStart = tryStart,
                TryEnd = tryEnd,
                HandlerStart = tryEnd,
                HandlerEnd = finallyEnd
            };
            mdMoveNext.Body.ExceptionHandlers.Add(handler);

            mdMoveNext.UpdateDebugInfoLastSequencePoint();
            mdMoveNext.SkipExceptionHandlerBlockSequencePoint(handler);

            return handler;
        }

        private static void StateMachinePersistentCheck(MethodDefinition methodDef, List<PoolItem> poolItems, Instruction previous)
        {
            if (VariablePersistentCheck(methodDef, poolItems, previous)) return;

            FieldPersistentCheck(methodDef, poolItems, previous);
        }

        private static bool FieldPersistentCheck(MethodDefinition methodDef, List<PoolItem> poolItems, Instruction previous)
        {
            if (!previous.IsLdfld()) return false;

            var fieldRef = (FieldReference)previous.Operand;
            poolItems.RemoveAll(x => x.Storing != null && x.Storing.Operand is FieldReference fr && fieldRef == fr);

            return true;
        }
    }
}
