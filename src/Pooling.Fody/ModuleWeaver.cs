using Cecil.AspectN;
using Cecil.AspectN.Matchers;
using Cecil.AspectN.Patterns.Parsers;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Pooling.Fody.AspectN.Patterns.Parsers;
using System.Collections.Generic;
using System.Linq;

namespace Pooling.Fody
{
    public partial class ModuleWeaver : SimulationModuleWeaver
    {
        private readonly Dictionary<TypeDefinition, Dictionary<string, ResetFunc?>> _cache = [];
        private ResetFuncManager _resetFuncManager;

        private TypeReference _trPool;

        private MethodDefinition _mdGet;
        private MethodDefinition _mdReturn;

        private Config _config;

#pragma warning disable CS8618, CS8601

        public ModuleWeaver() : this(false, null) { }

        public ModuleWeaver(bool testRun, Config? config = null) : base(testRun)
        {
            _config = config;
        }
#pragma warning restore CS8618, CS8601

        protected override bool Enabled()
        {
            LoadConfig();
            return _config.Enabled;
        }

        protected override void ExecuteInternal()
        {
            Parser.TypePrefixParsers.Add(new ForceResetTypePrefixParser());

            _resetFuncManager = new(ModuleDefinition, _tBooleanRef);

            foreach (var typeDef in ModuleDefinition.Types)
            {
                InspectType(typeDef);
            }
        }

        private void InspectMethod(MethodDefinition methodDef)
        {
            var instructions = methodDef.Body.Instructions;

            foreach (var instruction in instructions)
            {
                if (instruction.OpCode.Code != Code.Newobj || instruction.Operand is not MethodReference ctor || ctor.Parameters.Count != 0) continue;

                var declaringTypeRef = ctor.DeclaringType;
                if (declaringTypeRef is not TypeDefinition declaringTypeDef)
                {
                    declaringTypeDef = declaringTypeRef.Resolve();
                }
                if (!_cache.TryGetValue(declaringTypeDef, out var resetMap))
                {
                    resetMap = [];
                    _cache[declaringTypeDef] = resetMap;
                }
                var typeName = declaringTypeRef.FullName;
                if (!resetMap.TryGetValue(typeName, out var resetFunc))
                {
                    var typeSignature = SignatureParser.ParseType(declaringTypeRef);

                    if (_config.NonPooledTypes.Any(x => x.IsMatch(typeSignature)))
                    {
                        resetMap[typeName] = null;
                        continue;
                    }

                    foreach (var matcher in _config.PooledMethodTypes)
                    {
                        if (!matcher.DeclaringTypeMatcher.IsMatch(typeSignature)) continue;

                        MethodDefinition? zeroArgResetMethodDef = null;
                        foreach (var md in declaringTypeDef.Methods)
                        {
                            var methodSignature = SignatureParser.ParseMethod(md, _config.CompositeAccessibility);
                            if (matcher.IsMatch(methodSignature) && md.Parameters.Count == 0)
                            {
                                zeroArgResetMethodDef = md;
                                break;
                            }
                        }
                        if (zeroArgResetMethodDef != null)
                        {
                            resetFunc = _resetFuncManager.Create(zeroArgResetMethodDef);
                            break;
                        }
                    }

                    if (resetFunc == null)
                    {
                        if (declaringTypeRef.Implement(Constants.TYPE_IPoolItem))
                        {
                            resetFunc = _resetFuncManager.Create();
                        }
                        else
                        {
                            foreach (var matcher in _config.PooledTypes)
                            {
                                if (matcher.IsMatch(typeSignature))
                                {
                                    resetFunc = _resetFuncManager.Create();
                                    break;
                                }
                            }
                        }
                    }

                    resetMap[typeName] = resetFunc;
                }

                if (resetFunc == null) continue;

                // 将new操作替换为Pool操作
            }
        }

        private void LoadConfig()
        {
            if (_config != null) return;

            var enabled = GetConfigValue("true", "enabled");
            var compositeAccessibility = GetConfigValue("false", "composite-accessibility");
            var inclusiveMethods = GetConfigValue("", "inclusive-methods");
            var exclusiveMethods = GetConfigValue("", "exclusive-methods");
            var pooledMethodTypes = GetConfigValue("", "pooled-method-types");
            var pooledTypes = GetConfigValue("", "pooled-types");
            var nonPooledTypes = GetConfigValue("", "non-pooled-types");

            _config = new Config(enabled, compositeAccessibility, inclusiveMethods, exclusiveMethods, pooledMethodTypes, pooledTypes, nonPooledTypes);
            WriteConfigToDebug();

            void WriteConfigToDebug()
            {
                WriteDebug("======================Configuration Start======================");
                WriteDebug($"                enabled: {enabled}");
                WriteDebug($"composite-accessibility: {compositeAccessibility}");
                WriteDebug($"      inclusive-methods: {inclusiveMethods}");
                WriteDebug($"      exclusive-methods: {exclusiveMethods}");
                WriteDebug($"    pooled-method-types: {pooledMethodTypes}");
                WriteDebug($"           pooled-types: {pooledTypes}");
                WriteDebug($"       non-pooled-types: {nonPooledTypes}");
                WriteDebug("=======================Configuration End=======================");
            }
        }

        protected override void LoadBasicReference()
        {
            base.LoadBasicReference();

            _trPool = FindAndImportType(Constants.TYPE_Pool_1);
            _mdGet = _trPool.GetMethod(Constants.METHOD_Get, false);
            _mdReturn = _trPool.GetMethod(Constants.METHOD_Return, false);
        }

        public override IEnumerable<string> GetAssembliesForScanning()
        {
            foreach (var item in base.GetAssembliesForScanning())
            {
                yield return item;
            }

            if (_debugMode)
            {
                yield return "Pooling";
            }
            else
            {
                yield return "Pooling.Fody";
            }
        }
    }
}
