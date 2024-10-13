using Mono.Cecil;
using Mono.Cecil.Cil;
using Pooling.Fody.Visitors.Contexts;

namespace Pooling.Fody.Visitors
{
    internal class StateMachineVisitor(ModuleWeaver moduleWeaver, AnalysisStateMachineContext context) : PoolingVisitor<AnalysisStateMachineContext>(moduleWeaver, context.MdMoveNext, context)
    {
        protected override MethodDefinition InspectingMethodDef => _context.MdMoveNext;

        protected override bool VisitStfld(Instruction instruction)
        {
            var returned = base.VisitStfld(instruction);

            var fr = (FieldReference)instruction.Operand;
            var fieldAllocatingPoolItem = _counting.Decrease(2);
            if (fieldAllocatingPoolItem != null)
            {
                if (fr.DeclaringType.ToDefinition() == _context.TdStateMachine)
                {
                    fieldAllocatingPoolItem.Storing = instruction;
                    PoolItems.Add(fieldAllocatingPoolItem);
                    return returned;
                }
            }
            PersistentCheck(instruction);

            return returned;
        }

        protected override void PersistentCheck(Instruction instruction)
        {
            if (VariablePersistentCheck(instruction)) return;

            FieldPersistentCheck(instruction);
        }

        private bool FieldPersistentCheck(Instruction instruction)
        {
            var previous = instruction.Previous;

            if (!previous.IsLdfld()) return false;

            var fieldRef = (FieldReference)previous.Operand;
            PoolItems.RemoveAll(x => x.Storing != null && x.Storing.Operand is FieldReference fr && fieldRef == fr);

            return true;
        }
    }
}
