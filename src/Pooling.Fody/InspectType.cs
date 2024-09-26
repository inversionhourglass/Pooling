using Cecil.AspectN.Matchers;
using Cecil.AspectN;
using Mono.Cecil;
using System.Linq;

namespace Pooling.Fody
{
    partial class ModuleWeaver
    {
        private void InspectType(TypeDefinition typeDef)
        {
            if (typeDef.IsEnum || typeDef.IsInterface || typeDef.IsArray || typeDef.IsDelegate() || !typeDef.HasMethods) return;
            if (typeDef.IsCompilerGenerated()) return;

            var typeNonPooledMatcher = TryResolveNonPooledMatcher(typeDef.CustomAttributes);
            if (typeNonPooledMatcher == null) return;

            var typeSignature = SignatureParser.ParseType(typeDef);
            var inspectMatchers = MatchType(_config.Inspects, typeSignature);
            if (inspectMatchers != null && inspectMatchers.Length == 0) return; // 存在inspect表达式，但是一个都匹配不上，那么当前类型就不在池化检测范围
            
            var notInspectMatchers = MatchType(_config.NotInspects, typeSignature);

            foreach (var methodDef in typeDef.Methods)
            {
                InspectMethod(methodDef, typeNonPooledMatcher, inspectMatchers, notInspectMatchers);
            }

            if (typeDef.HasNestedTypes)
            {
                foreach (var nestedTypeDef in typeDef.NestedTypes.ToArray())
                {
                    InspectType(nestedTypeDef);
                }
            }
        }

        private static IMatcher[]? MatchType(IMatcher[] matchers, TypeSignature typeSignature)
        {
            if (matchers.Length == 0) return null;

            return matchers.Where(x => x is not ExecutionMatcher || !x.SupportDeclaringTypeMatch || x.DeclaringTypeMatcher.IsMatch(typeSignature)).ToArray();
        }
    }
}
