using Mono.Cecil;
using Mono.Cecil.Cil;
using Pooling.Fody.Visitors.Contexts;

namespace Pooling.Fody.Visitors
{
    internal class SyncMethodVisitor(ModuleWeaver moduleWeaver, AnalysisSyncMethodContext context) : PoolingVisitor<AnalysisSyncMethodContext>(moduleWeaver, context.MethodSignature.Definition, context)
    {
        protected override MethodDefinition InspectingMethodDef => _context.MethodSignature.Definition;

        protected override bool VisitStfld(Instruction instruction)
        {
            var returned = base.VisitStfld(instruction);

            VariablePersistentCheck(instruction);
            _counting.Decrease(2);

            return returned;
        }

        protected override void PersistentCheck(Instruction instruction)
        {
            VariablePersistentCheck(instruction);
        }
    }
}
