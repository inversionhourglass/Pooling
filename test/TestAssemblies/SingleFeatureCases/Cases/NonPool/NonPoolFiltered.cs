using Pooling;
using SingleFeatureCases.PoolItems.Interfaces;

[assembly:NonPooled(Types = [typeof(AssemblyNonPoolTypeFiltered)], Pattern = "SingleFeatureCases.PoolItems.*.Assembly*PatternFiltered")]

namespace SingleFeatureCases.Cases.NonPool
{
    [NonPooled(Types = [typeof(ClassNonPoolTypeFiltered)], Pattern = "*..Interfaces.ClassNonPoolPatternFiltered")]
    public class NonPoolFiltered : Stateful<NonPoolFiltered>
    {
        public static void AssemblyFiltered()
        {
            var pooling = PoolingResult.New();

            var assemblyType = new AssemblyNonPoolTypeFiltered();
            var assemblyPattern = new AssemblyNonPoolPatternFiltered();
            var classType = new ClassNonPoolTypeFiltered();
            var classPattern = new ClassNonPoolPatternFiltered();
            var methodType = new MethodNonPoolTypeFiltered();
            var methodPattern = new MethodNonPoolPatternFiltered();
            AvoidOptimize(assemblyType, assemblyPattern, classType, classPattern, methodType, methodPattern);

            pooling.ShouldPooled(methodType, methodPattern);
            pooling.ShouldNotPooled(assemblyType, assemblyPattern, classType, classPattern);
            PoolingResult = pooling;
        }

        public static void ClassFiltered()
        {
            var pooling = PoolingResult.New();

            var assemblyType = new AssemblyNonPoolTypeFiltered();
            var assemblyPattern = new AssemblyNonPoolPatternFiltered();
            var classType = new ClassNonPoolTypeFiltered();
            var classPattern = new ClassNonPoolPatternFiltered();
            var methodType = new MethodNonPoolTypeFiltered();
            var methodPattern = new MethodNonPoolPatternFiltered();
            AvoidOptimize(assemblyType, assemblyPattern, classType, classPattern, methodType, methodPattern);

            pooling.ShouldPooled(methodType, methodPattern);
            pooling.ShouldNotPooled(assemblyType, assemblyPattern, classType, classPattern);
            PoolingResult = pooling;
        }

        [NonPooled(Types = [typeof(MethodNonPoolTypeFiltered)])]
        public static void MethodTypeFiltered()
        {
            var pooling = PoolingResult.New();

            var assemblyType = new AssemblyNonPoolTypeFiltered();
            var assemblyPattern = new AssemblyNonPoolPatternFiltered();
            var classType = new ClassNonPoolTypeFiltered();
            var classPattern = new ClassNonPoolPatternFiltered();
            var methodType = new MethodNonPoolTypeFiltered();
            var methodPattern = new MethodNonPoolPatternFiltered();
            AvoidOptimize(assemblyType, assemblyPattern, classType, classPattern, methodType, methodPattern);

            pooling.ShouldPooled(methodPattern);
            pooling.ShouldNotPooled(assemblyType, assemblyPattern, classType, classPattern, methodType);
            PoolingResult = pooling;
        }

        [NonPooled(Pattern = "MethodNonPoolPatternFiltered")]
        public static void MethodPatternFiltered()
        {
            var pooling = PoolingResult.New();

            var assemblyType = new AssemblyNonPoolTypeFiltered();
            var assemblyPattern = new AssemblyNonPoolPatternFiltered();
            var classType = new ClassNonPoolTypeFiltered();
            var classPattern = new ClassNonPoolPatternFiltered();
            var methodType = new MethodNonPoolTypeFiltered();
            var methodPattern = new MethodNonPoolPatternFiltered();
            AvoidOptimize(assemblyType, assemblyPattern, classType, classPattern, methodType, methodPattern);

            pooling.ShouldPooled(methodType);
            pooling.ShouldNotPooled(assemblyType, assemblyPattern, classType, classPattern, methodPattern);
            PoolingResult = pooling;
        }
    }
}
