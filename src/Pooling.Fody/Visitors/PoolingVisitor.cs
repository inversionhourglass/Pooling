using Cecil.AspectN.Matchers;
using Cecil.AspectN;
using Fody;
using Fody.Inspectors;
using Mono.Cecil.Cil;
using Mono.Cecil;
using Pooling.Fody.Visitors.Contexts;
using System.Linq;
using System.Collections.Generic;

namespace Pooling.Fody.Visitors
{
    internal abstract class PoolingVisitor<TContext>(ModuleWeaver moduleWeaver, MethodDefinition methodDef, TContext context) : MethodBodyVisitor(methodDef.Body) where TContext : AnalysisContext
    {
        protected readonly ModuleWeaver _moduleWeaver = moduleWeaver;
        protected readonly StackCounting _counting = new();
        protected readonly TContext _context = context;
        protected readonly List<PoolItem> _poolItems = [];
        protected readonly HashSet<Instruction> _persistents = [];

        protected abstract MethodDefinition InspectingMethodDef { get; }

        public PoolItem[] GetPoolItems()
        {
            foreach (var persistent in _persistents)
            {
                PersistentCheck(persistent);
            }

            return _poolItems.Distinct().ToArray();
        }

        protected override bool VisitNewobj(Instruction instruction)
        {
            var returned = base.VisitNewobj(instruction);

            if (instruction.Operand is MethodReference ctorRef && ctorRef.HasParameters)
            {
                _counting.Decrease(ctorRef.Parameters.Count);
            }
            _counting.Increase();
            var detectedPoolItem = InspectInstruction(instruction);
            if (detectedPoolItem != null)
            {
                _counting.Add(detectedPoolItem);
            }

            return returned;
        }

        #region Stloc
        protected override bool VisitStloc(Instruction instruction)
        {
            var returned = base.VisitStloc(instruction);

            Stloc(instruction);

            return returned;
        }

        protected override bool VisitStloc_S(Instruction instruction)
        {
            var returned = base.VisitStloc_S(instruction);

            Stloc(instruction);

            return returned;
        }

        protected override bool VisitStloc_0(Instruction instruction)
        {
            var returned = base.VisitStloc_0(instruction);

            Stloc(instruction);

            return returned;
        }

        protected override bool VisitStloc_1(Instruction instruction)
        {
            var returned = base.VisitStloc_1(instruction);

            Stloc(instruction);

            return returned;
        }

        protected override bool VisitStloc_2(Instruction instruction)
        {
            var returned = base.VisitStloc_2(instruction);

            Stloc(instruction);

            return returned;
        }

        protected override bool VisitStloc_3(Instruction instruction)
        {
            var returned = base.VisitStloc_3(instruction);

            Stloc(instruction);

            return returned;
        }

        private void Stloc(Instruction instruction)
        {
            var allocatingPoolItem = _counting.Decrease();
            if (allocatingPoolItem != null)
            {
                allocatingPoolItem.Storing = instruction;
                _poolItems.Add(allocatingPoolItem);
                _moduleWeaver.WriteDebug($"Found pooling item variable in {_context.MethodSignature.Definition} at offset {instruction.Offset}");
            }
        }
        #endregion Stloc

        protected override bool VisitCall(Instruction instruction)
        {
            var returned = base.VisitCall(instruction);

            Call(instruction);

            return returned;
        }

        protected override bool VisitCallvirt(Instruction instruction)
        {
            var returned = base.VisitCallvirt(instruction);

            Call(instruction);

            return returned;
        }

        private void Call(Instruction instruction)
        {
            var mr = (MethodReference)instruction.Operand;
            var md = mr.ToDefinition();

            if (md.IsSetter) _persistents.Add(instruction);

            var staticAmount = md.IsStatic ? 0 : 1;
            var amount = mr.Parameters.Count + staticAmount;
            _counting.Decrease(amount);
            if (!mr.ReturnType.IsVoid())
            {
                _counting.Increase();
            }
        }

        protected override bool VisitStsfld(Instruction instruction)
        {
            var returned = base.VisitStsfld(instruction);

            _persistents.Add(instruction);
            _counting.Decrease();

            return returned;
        }

        protected override bool VisitCalli(Instruction instruction)
        {
            var returned = base.VisitCalli(instruction);

            var callSite = (CallSite)instruction.Operand;
            _counting.Decrease(callSite.Parameters.Count + 1);
            if (!callSite.ReturnType.IsVoid())
            {
                _counting.Increase();
            }

            return returned;
        }

        protected PoolItem? InspectInstruction(Instruction newObj)
        {
            if (newObj.Operand is not MethodReference ctor || ctor.Parameters.Count != 0) return null;

            var methodDef = _context.MethodSignature.Definition;
            var typeRef = ctor.DeclaringType;
            var typeSignature = SignatureParser.ParseType(typeRef);

            if (_context.AssemblyNonPooledMatcher != null && _context.AssemblyNonPooledMatcher.Any(x => x.IsMatch(typeSignature)))
            {
                _moduleWeaver.WriteDebug($"[{methodDef}] The type {typeSignature.Reference} is marked as NonPooledAttribute in the assembly level, so the new operation will not be replaced by the pool operation.");
                return null;
            }
            if (_context.TypeNonPooledMatcher.Any(x => x.IsMatch(typeSignature)))
            {
                _moduleWeaver.WriteDebug($"[{methodDef}] The type {typeSignature.Reference} is marked as NonPooledAttribute in the type level, so the new operation will not be replaced by the pool operation.");
                return null;
            }
            if (_context.MethodNonPooledMatcher.Any(x => x.IsMatch(typeSignature)))
            {
                _moduleWeaver.WriteDebug($"[{methodDef}] The type {typeSignature.Reference} is marked as NonPooledAttribute in the method level, so the new operation will not be replaced by the pool operation.");
                return null;
            }

            if (typeRef.Implement(Constants.TYPE_IPoolItem))
            {
                var exclusive = PoolItemExclusive.Resolve(typeRef);
                var excluded = exclusive.IsExcluded(_context.MethodSignature);
                if (excluded)
                {
                    _moduleWeaver.WriteInfo($"Detected pooling item {typeRef} that implements IPoolItem in {_context.MethodSignature.Definition} at offset {newObj.Offset}, but it is excluded by PoolingExclusiveAttribute.");
                    return null;
                }
                return new(newObj, typeRef);
            }

            ITypeMatcher? stateless = null;
            foreach (var item in _context.Items)
            {
                if (item.Pattern != null)
                {
                    if (!item.Pattern.SupportDeclaringTypeMatch || item.Pattern.DeclaringTypeMatcher.IsMatch(typeSignature))
                    {
                        var resetMethodDef = typeSignature.FindMethod(item.Pattern, _moduleWeaver._config.CompositeAccessibility);
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

        protected abstract void PersistentCheck(Instruction instruction);

        protected bool VariablePersistentCheck(Instruction instruction)
        {
            var previous = instruction.Previous;

            if (!previous.IsLdloc()) return false;

            if (previous.TryResolveVariable(InspectingMethodDef, out var variable))
            {
                _poolItems.RemoveAll(x =>
                {
                    var matched = x.Storing != null && x.Storing.TryResolveVariable(InspectingMethodDef, out var v) && v == variable;
                    if (matched)
                    {
                        _moduleWeaver.WriteDebug($"The pooling item variable allocated at offset {x.Storing!.Offset} is saved to the field or property at offset {instruction.Offset}, so it will not be able to be pooled.");
                    }
                    return matched;
                });
            }

            return true;
        }
    }
}
