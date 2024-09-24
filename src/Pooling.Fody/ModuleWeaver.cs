using Cecil.AspectN.Matchers;
using Cecil.AspectN.Patterns.Parsers;
using Fody;
using Mono.Cecil;
using Pooling.Fody.AspectN.Patterns.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pooling.Fody
{
    public partial class ModuleWeaver : SimulationModuleWeaver
    {
        private readonly Dictionary<TypeDefinition, Dictionary<string, ResetFunc?>> _cache = [];
        private ResetFuncManager _resetFuncManager;

        private TypeReference _trPool;

        private MethodReference _mrGet;
        private MethodReference _mrReturn;

        private ITypeMatcher[]? _assemblyNonPooledMatcher;

        private Config _config;

#pragma warning disable CS8618, CS8601

        public ModuleWeaver(bool testRun) : base(testRun) { }
#pragma warning restore CS8618, CS8601

        protected override bool Enabled()
        {
            LoadConfig();
            return _config.Enabled;
        }

        protected override void ExecuteInternal()
        {
            if (TryResolveAssemblyNonPooledMatcher()) return;

            Parser.TypePrefixParsers.Add(new ForceResetTypePrefixParser());

            _resetFuncManager = new(this, _tBooleanRef);

            foreach (var typeDef in ModuleDefinition.Types.ToArray())
            {
                if (typeDef.IsEnum || typeDef.IsInterface || typeDef.IsArray || typeDef.IsDelegate() || !typeDef.HasMethods) continue;
                if (typeDef.IsCompilerGenerated()) continue;

                InspectType(typeDef);
            }
        }

        /// <summary>
        /// 返回true表示存在全局不池化的NonPooledAttribute，后续直接返回，结束Pooling检查工作即可
        /// </summary>
        private bool TryResolveAssemblyNonPooledMatcher()
        {
            var assemblyNonPooledMatcher = TryResolveNonPooledMatcher(ModuleDefinition.CustomAttributes);
            if (assemblyNonPooledMatcher == null)
            {
                WriteInfo("Exit pooling operation by module level NonPooledAttribute.");
                return true;
            }
            var moduleNonPooledMatcher = TryResolveNonPooledMatcher(ModuleDefinition.Assembly.CustomAttributes);
            if (moduleNonPooledMatcher == null)
            {
                WriteInfo("Exit pooling operation by assembly level NonPooledAttribute.");
                return true;
            }

            _assemblyNonPooledMatcher = [.. assemblyNonPooledMatcher, .. moduleNonPooledMatcher];
            return false;
        }

        /// <summary>
        /// 返回null表示至少有一个未对属性进行赋值的NonPooledAttribute，无任何配置的NonPooledAttribute表示整体不进行池化操作，不针对某一池化类型
        /// </summary>
        /// <returns>池化类型匹配器，表示当前目标哪些池化类型不进行池化操作</returns>
        private static ITypeMatcher[]? TryResolveNonPooledMatcher(Mono.Collections.Generic.Collection<CustomAttribute> attributes)
        {
            var matchers = new List<ITypeMatcher>();

            foreach (var attribute in attributes)
            {
                if (attribute.Is(Constants.TYPE_NonPooledAttribute))
                {
                    var beforeCount = matchers.Count;
                    if (attribute.Properties.Count == 0) return null;

                    foreach (var property in attribute.Properties)
                    {
                        if (property.Name == Constants.PROP_Types)
                        {
                            if (property.Argument.Value is not TypeReference typeRef) throw new ArgumentException($"Cannot parse the Types property value of NonPooledAttribute to a TypeReference instance, the actual type is {property.Argument.Value.GetType()}");

                            matchers.Add(new TypeReferenceMatcher(typeRef));
                        }
                        else if (property.Name == Constants.PROP_Pattern)
                        {
                            if (property.Argument.Value is not string pattern) throw new ArgumentException($"Cannot parse the Types property value of NonPooledAttribute to a string instance, the actual type is {property.Argument.Value.GetType()}");

                            matchers.Add(new TypeMatcher(pattern));
                        }
                    }
                    if (matchers.Count == beforeCount) return null;
                }
            }

            return matchers.Select(x => x.Cached()).ToArray();
        }

        private void LoadConfig()
        {
            if (_config != null) return;

            var enabled = GetConfigValue("true", "enabled");
            var compositeAccessibility = GetConfigValue("false", "composite-accessibility");
            var inclusives = new List<string>();
            var exclusives = new List<string>();
            var items = new List<Config.Item>();
            var xInclusives = Config.Element("Inclusives");
            var xExclusives = Config.Element("Exclusives");
            var xItems = Config.Element("Items");
            if (xInclusives != null)
            {
                foreach (var xInclusive in xInclusives.Elements())
                {
                    if (xInclusive.Name == "Inclusive" && !string.IsNullOrEmpty(xInclusive.Value))
                    {
                        inclusives.Add(xInclusive.Value);
                    }
                }
            }
            if (xExclusives != null)
            {
                foreach (var xExclusive in xExclusives.Elements())
                {
                    if (xExclusive.Name == "Exclusive" && !string.IsNullOrEmpty(xExclusive.Value))
                    {
                        exclusives.Add(xExclusive.Value);
                    }
                }
            }
            if (xItems != null)
            {
                foreach(var xItem in xItems.Elements())
                {
                    if (xItem.Name != "Item") continue;

                    var pattern = xItem.Attribute("pattern")?.Value;
                    var stateless = xItem.Attribute("stateless")?.Value;
                    var apply = xItem.Attribute("apply")?.Value;
                    var exclusive = xItem.Attribute("exclusive")?.Value;
                    if (pattern != null || stateless != null)
                    {
                        items.Add(new(pattern, stateless, apply, exclusive));
                    }
                }
            }

            _config = new Config(enabled, compositeAccessibility, inclusives.ToArray(), exclusives.ToArray(), items.ToArray());
            WriteConfigToDebug();

            void WriteConfigToDebug()
            {
                WriteDebug("======================Configuration Start======================");
                WriteDebug(Config.ToString());
                WriteDebug("=======================Configuration End=======================");
            }
        }

        protected override void LoadBasicReference()
        {
            base.LoadBasicReference();

            _trPool = FindAndImportType(Constants.TYPE_Pool_1);
            _mrGet = this.Import(_trPool.GetMethod(Constants.METHOD_Get, false));
            _mrReturn = this.Import(_trPool.GetMethod(Constants.METHOD_Return, false));
        }

        public override IEnumerable<string> GetAssembliesForScanning()
        {
            foreach (var item in base.GetAssembliesForScanning())
            {
                yield return item;
            }

            if (_testRun)
            {
                yield return "Pooling";
            }
            //else
            //{
            //    yield return "Pooling.Fody";
            //}
        }
    }
}
