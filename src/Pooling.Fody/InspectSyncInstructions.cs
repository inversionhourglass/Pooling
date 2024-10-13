using Cecil.AspectN;
using Cecil.AspectN.Matchers;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Pooling.Fody.Visitors;
using Pooling.Fody.Visitors.Contexts;
using System.Collections.Generic;
using System.Linq;

namespace Pooling.Fody
{
    partial class ModuleWeaver
    {
        private void InspectSyncInstructions(MethodSignature methodSignature, ITypeMatcher[] typeNonPooledMatcher, ITypeMatcher[] methodNonPooledMatcher, Config.Item[] items)
        {
            var methodDef = methodSignature.Definition;
            var context = new AnalysisSyncMethodContext(methodSignature, _assemblyNonPooledMatcher, typeNonPooledMatcher, methodNonPooledMatcher, items);
            var visitor = new SyncMethodVisitor(this, context);
            visitor.Visit();

            var poolItems = visitor.GetPoolItems();

            if (poolItems.Length == 0) return;

            Pooling(poolItems, false);

            var handler = methodDef.BuildOutermostExceptionHandler(ExceptionHandlerType.Finally);

            PoolingRecycle(methodDef, handler, poolItems);

            methodDef.Body.InitLocals = true;
            methodDef.Body.OptimizePlus();
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
                    .. poolItem.CallResetMethod(this, genericArguments, nextPoolItemRecycleStart),
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
