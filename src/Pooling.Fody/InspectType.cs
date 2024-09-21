using Cecil.AspectN.Matchers;
using Cecil.AspectN;
using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace Pooling.Fody
{
    partial class ModuleWeaver
    {
        private void InspectType(TypeDefinition typeDef)
        {
            MethodMatcher[]? inclusiveMethods = null;
            MethodMatcher[]? exclusiveMethods = null;

            if (_config.InclusiveMethods.Length > 0)
            {
                inclusiveMethods = MatchType(_config.InclusiveMethods, typeDef);
                if (inclusiveMethods.Length == 0) return;
            }
            if (_config.ExclusiveMethods.Length > 0)
            {
                exclusiveMethods = MatchType(_config.ExclusiveMethods, typeDef);
            }

            foreach (var methodDef in typeDef.Methods)
            {
                var signature = SignatureParser.ParseMethod(methodDef, _config.CompositeAccessibility);
                var exclusivePattern = exclusiveMethods?.FirstOrDefault(x => x.IsMatch(signature)).Pattern;
                if (exclusivePattern != null)
                {
                    WriteInfo($"{methodDef} is excluded by the exclusive-methods pattern ({exclusivePattern}).");
                    continue;
                }

                if (inclusiveMethods != null)
                {
                    var inclusivePattern = inclusiveMethods.FirstOrDefault(x => x.IsMatch(signature)).Pattern;
                    if (inclusivePattern == null) continue;
                    WriteInfo($"{methodDef} is included by the inclusive-methods pattern ({inclusivePattern}).");
                }
                else
                {
                    WriteInfo($"{methodDef} is included because the inclusive-methods configuration is absent.");
                }

                InspectMethod(methodDef);
            }

            if (typeDef.HasNestedTypes)
            {
                foreach (var nestedTypeDef in typeDef.NestedTypes)
                {
                    InspectType(nestedTypeDef);
                }
            }

            static MethodMatcher[] MatchType(MethodMatcher[] matchers, TypeDefinition typeDef)
            {
                var matchedMatcher = new List<MethodMatcher>();
                var typeSignature = SignatureParser.ParseType(typeDef);
                foreach (var matcher in matchers)
                {
                    if (matcher.DeclaringTypeMatcher.IsMatch(typeSignature))
                    {
                        matchedMatcher.Add(matcher);
                    }
                }

                return matchedMatcher.ToArray();
            }
        }
    }
}
