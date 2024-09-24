using Cecil.AspectN;
using Cecil.AspectN.Matchers;
using Fody;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pooling.Fody
{
    partial class ModuleWeaver
    {
        private void InspectMethod(MethodDefinition methodDef, ITypeMatcher[] typeNonPooledMatcher, IMatcher[]? inclusiveMatchers, IMatcher[]? exclusiveMatchers)
        {
            try
            {
                InspectMethodCore(methodDef, typeNonPooledMatcher, inclusiveMatchers, exclusiveMatchers);
            }
            catch (FodyWeavingException ex)
            {
                ex.MethodDef ??= methodDef;
                throw;
            }
            catch (Exception ex)
            {
                throw new FodyWeavingException(ex.ToString(), methodDef);
            }
        }

        private void InspectMethodCore(MethodDefinition methodDef, ITypeMatcher[] typeNonPooledMatcher, IMatcher[]? inclusiveMatchers, IMatcher[]? exclusiveMatchers)
        {
            var methodNonPooledMatcher = TryResolveNonPooledMatcher(methodDef.CustomAttributes);
            if (methodNonPooledMatcher == null) return;

            var signature = SignatureParser.ParseMethod(methodDef, _config.CompositeAccessibility);

            if (IsExcluded(signature, exclusiveMatchers)) return;

            if (!IsIncluded(signature, inclusiveMatchers)) return;

            var items = new List<Config.Item>();
            foreach (var item in _config.Items)
            {
                if (item.Exclusive != null && item.Exclusive.IsMatch(signature)) continue;

                if (item.Apply != null && !item.Apply.IsMatch(signature)) continue;

                items.Add(item);
            }

            if (methodDef.TryResolveStateMachine(out var stateMachineTypeDef))
            {
                InspectAsyncInstructions(stateMachineTypeDef!, typeNonPooledMatcher, methodNonPooledMatcher, items.ToArray());
            }
            else
            {
                InspectSyncInstructions(methodDef, typeNonPooledMatcher, methodNonPooledMatcher, items.ToArray());
            }
        }

        private bool IsExcluded(MethodSignature signature, IMatcher[]? exclusiveMatchers)
        {
            var exclusiveMatcher = exclusiveMatchers?.FirstOrDefault(x => x.IsMatch(signature));
            if (exclusiveMatcher != null)
            {
                WriteDebug($"{signature.Definition} is excluded by the global exclusive pattern ({exclusiveMatcher}).");
                return true;
            }

            return false;
        }

        private bool IsIncluded(MethodSignature signature, IMatcher[]? inclusiveMatchers)
        {
            if (inclusiveMatchers != null)
            {
                var inclusiveMatcher = inclusiveMatchers.FirstOrDefault(x => x.IsMatch(signature));
                if (inclusiveMatcher == null)
                {
                    WriteDebug($"{signature.Definition} is not included in the global inclusive pattern ({inclusiveMatcher}).");
                    return false;
                }
            }

            return true;
        }
    }
}
