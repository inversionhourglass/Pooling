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
        private void InspectMethod(MethodDefinition methodDef, ITypeMatcher[] typeNonPooledMatcher, IMatcher[]? inspectMatchers, IMatcher[]? notInspectMatchers)
        {
            try
            {
                InspectMethodCore(methodDef, typeNonPooledMatcher, inspectMatchers, notInspectMatchers);
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

        private void InspectMethodCore(MethodDefinition methodDef, ITypeMatcher[] typeNonPooledMatcher, IMatcher[]? inspectMatchers, IMatcher[]? notInspectMatchers)
        {
            if (methodDef.IsAbstract) return;

            // Windows api. Extern method with DllImportAttribute
            if (methodDef.HasPInvokeInfo || methodDef.IsPInvokeImpl) return;

            var methodNonPooledMatcher = TryResolveNonPooledMatcher(methodDef.CustomAttributes);
            if (methodNonPooledMatcher == null) return;

            var signature = SignatureParser.ParseMethod(methodDef, _config.CompositeAccessibility);

            if (IsExcluded(signature, notInspectMatchers)) return;

            if (!IsIncluded(signature, inspectMatchers)) return;

            var items = new List<Config.Item>();
            foreach (var item in _config.Items)
            {
                if (item.NotInspect != null && item.NotInspect.IsMatch(signature)) continue;

                if (item.Inspect != null && !item.Inspect.IsMatch(signature)) continue;

                items.Add(item);
            }

            if (methodDef.TryResolveStateMachine(out var stateMachineTypeDef, out var stateMachineAttribute))
            {
                InspectStateMachineInstructions(signature, stateMachineTypeDef!, stateMachineAttribute, typeNonPooledMatcher, methodNonPooledMatcher, items.ToArray());
            }
            else
            {
                InspectSyncInstructions(signature, typeNonPooledMatcher, methodNonPooledMatcher, items.ToArray());
            }
        }

        private bool IsExcluded(MethodSignature signature, IMatcher[]? notInspectMatchers)
        {
            var notInspectMatcher = notInspectMatchers?.FirstOrDefault(x => x.IsMatch(signature));
            if (notInspectMatcher != null)
            {
                WriteDebug($"{signature.Definition} is excluded by the global NotInspect pattern ({notInspectMatcher}).");
                return true;
            }

            return false;
        }

        private bool IsIncluded(MethodSignature signature, IMatcher[]? inspectMatchers)
        {
            if (inspectMatchers != null)
            {
                var inspectMatcher = inspectMatchers.FirstOrDefault(x => x.IsMatch(signature));
                if (inspectMatcher == null)
                {
                    WriteDebug($"{signature.Definition} is not included in the global Inspect pattern ({inspectMatcher}).");
                    return false;
                }
            }

            return true;
        }
    }
}
