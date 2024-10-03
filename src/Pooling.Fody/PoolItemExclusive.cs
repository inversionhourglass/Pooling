using Cecil.AspectN;
using Cecil.AspectN.Matchers;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
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
                var typeDef = typeRef.ToDefinition();
                var attrPoolingExclusive = typeDef.CustomAttributes.FirstOrDefault(x => x.Is(Constants.TYPE_PoolingExclusiveAttribute));

                var types = attrPoolingExclusive == null ? [] : ResolveTypes(attrPoolingExclusive);
                var pattern = attrPoolingExclusive == null ? null : ResolvePattern(attrPoolingExclusive);
                exclusive = new(types, pattern);
                _Cache[key] = exclusive;
            }

            return exclusive;
        }

        private static TypeReference[] ResolveTypes(CustomAttribute attrPoolingExclusive)
        {
            var pTypes = attrPoolingExclusive.Properties.FirstOrDefault(x => x.Name == Constants.PROP_Types);
            if (pTypes.Name == null || pTypes.Argument.Value is not CustomAttributeArgument[] types) return [];

            return types.Select(x => x.Value is TypeReference typeRef ? typeRef : throw new ArgumentException($"Cannot parse the Types property value of PoolingExclusiveAttribute to a TypeReference instance, the actual type is {x.Value.GetType()}")).ToArray();
        }

        private static string? ResolvePattern(CustomAttribute attrPoolingExclusive)
        {
            var pPattern = attrPoolingExclusive.Properties.FirstOrDefault(x => x.Name == Constants.PROP_Pattern);
            return pPattern.Name == null || pPattern.Argument.Value is not string pattern ? null : pattern;
        }
    }
}
