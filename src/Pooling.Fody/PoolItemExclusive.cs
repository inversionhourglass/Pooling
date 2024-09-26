using Cecil.AspectN;
using Cecil.AspectN.Matchers;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Linq;

namespace Pooling.Fody
{
    internal class PoolItemExclusive(TypeReference[] types, string? pattern)
    {
        private static readonly Dictionary<string, PoolItemExclusive> _Cache = [];

        public TypeReference[] Types { get; } = types;

        public IMatcher? Patterns { get; } = pattern == null ? null : PatternParser.Parse(pattern).Cached();

        public bool IsExcluded(MethodSignature method)
        {
            return Types.Any(x => x.StrictEqual(method.Definition.DeclaringType)) || (Patterns != null && Patterns.IsMatch(method));
        }

        public static PoolItemExclusive Resolve(TypeReference typeRef)
        {
            var key = $"{typeRef.Scope} {typeRef}";
            if (!_Cache.TryGetValue(key, out var exclusive))
            {
                var types = ResolveTypes(typeRef);
                var pattern = ResolvePattern(typeRef);
                exclusive = new(types, pattern);
                _Cache[key] = exclusive;
            }

            return exclusive;
        }

        private static TypeReference[] ResolveTypes(TypeReference typeRef)
        {
            var mdGetExclusiveTypes = typeRef.GetMethod(true, md => md.IsGetter && md.Name == Constants.Getter(Constants.PROP_ExclusiveTypes));
            if (mdGetExclusiveTypes == null) return [];

            var exclusiveTypes = new List<TypeReference>();
            foreach (var instruction in mdGetExclusiveTypes.Body.Instructions)
            {
                if (instruction.OpCode.Code == Code.Ldtoken && instruction.Operand is TypeReference tr)
                {
                    exclusiveTypes.Add(tr);
                }
            }

            return exclusiveTypes.ToArray();
        }

        private static string? ResolvePattern(TypeReference typeRef)
        {
            var mdGetExclusivePattern = typeRef.GetMethod(true, md => md.IsGetter && md.Name == Constants.Getter(Constants.PROP_ExclusivePattern));
            if (mdGetExclusivePattern == null) return null;

            foreach (var instruction in mdGetExclusivePattern.Body.Instructions)
            {
                if (instruction.OpCode.Code == Code.Ldstr && instruction.Operand is string pattern)
                {
                    return pattern;
                }
            }

            return null;
        }
    }
}
