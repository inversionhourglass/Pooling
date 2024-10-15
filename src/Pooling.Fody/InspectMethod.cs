using Cecil.AspectN;
using Cecil.AspectN.Matchers;
using Fody;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

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
            if (methodNonPooledMatcher == null)
            {
                WriteInfo($"Skip inspecting the method {methodDef} based on the method-level NonPooledAttribute.");
                return;
            }

            var signature = SignatureParser.ParseMethod(methodDef, _config.CompositeAccessibility);

            if (IsExcluded(signature, notInspectMatchers)) return;

            if (!IsIncluded(signature, inspectMatchers)) return;

            var items = new List<Config.Item>();
            foreach (var item in _config.Items)
            {
                if (item.NotInspect != null && item.NotInspect.IsMatch(signature))
                {
                    WriteDebug($"PoolItem({item.PatternOrStateless}) skips inspecting the method {signature.Definition} based on its not-inspect configuration ({item.NotInspect}).");
                    continue;
                }

                if (item.Inspect != null && !item.Inspect.IsMatch(signature))
                {
                    WriteDebug($"PoolItem({item.PatternOrStateless}) skips inspecting the method {signature.Definition} based on its inspect configuration ({item.Inspect}).");
                    continue;
                }

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
                WriteDebug($"Skip inspecting the method {signature.Definition} based on the NotInspect configuration({notInspectMatcher}).");
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
                    WriteDebug($"Skip inspecting the method {signature.Definition} based on the Inspects configuration.");
                    return false;
                }
            }

            return true;
        }
    }
}
