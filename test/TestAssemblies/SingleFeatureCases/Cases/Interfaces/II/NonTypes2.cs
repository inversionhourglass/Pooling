using Pooling;
using SingleFeatureCases.PoolItems.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SingleFeatureCases.Cases.Interfaces.II
{
    public class NonTypes2 : Stateful<NonTypes2>
    {
        [NonPooled]
        public static object? Activator { get; }

        static NonTypes2()
        {
            var pooling = PoolingResult.New();

            var any = new InterfaceAny();
            var non = new InterfaceNon();
            var nonPattern = new InterfaceNonPattern();
            var nonTypes = new InterfaceNonTypes();
            AvoidOptimize(any, non, nonPattern, nonTypes);

            pooling.ShouldPooled(any, nonPattern);
            pooling.ShouldNotPooled(non, nonTypes);
            PoolingResult = pooling;
        }

        public NonTypes2()
        {
            var pooling = PoolingResult.New();

            var any = new InterfaceAny();
            var non = new InterfaceNon();
            var nonPattern = new InterfaceNonPattern();
            var nonTypes = new InterfaceNonTypes();
            AvoidOptimize(any, non, nonPattern, nonTypes);

            pooling.ShouldPooled(any, nonPattern);
            pooling.ShouldNotPooled(non, nonTypes);
            PoolingResult = pooling;
        }

        public object? Prop
        {
            get
            {
                var pooling = PoolingResult.New();

                var any = new InterfaceAny();
                var non = new InterfaceNon();
                var nonPattern = new InterfaceNonPattern();
                var nonTypes = new InterfaceNonTypes();
                AvoidOptimize(any, non, nonPattern, nonTypes);

                pooling.ShouldPooled(any, nonPattern);
                pooling.ShouldNotPooled(non, nonTypes);
                PoolingResult = pooling;

                return default;
            }
            set
            {
                var pooling = PoolingResult.New();

                var any = new InterfaceAny();
                var non = new InterfaceNon();
                var nonPattern = new InterfaceNonPattern();
                var nonTypes = new InterfaceNonTypes();
                AvoidOptimize(any, non, nonPattern, nonTypes);

                pooling.ShouldPooled(any, nonPattern);
                pooling.ShouldNotPooled(non, nonTypes);
                PoolingResult = pooling;
            }
        }

        public static object? StaticProp
        {
            get
            {
                var pooling = PoolingResult.New();

                var any = new InterfaceAny();
                var non = new InterfaceNon();
                var nonPattern = new InterfaceNonPattern();
                var nonTypes = new InterfaceNonTypes();
                AvoidOptimize(any, non, nonPattern, nonTypes);

                pooling.ShouldPooled(any, nonPattern);
                pooling.ShouldNotPooled(non, nonTypes);
                PoolingResult = pooling;

                return default;
            }
            set
            {
                var pooling = PoolingResult.New();

                var any = new InterfaceAny();
                var non = new InterfaceNon();
                var nonPattern = new InterfaceNonPattern();
                var nonTypes = new InterfaceNonTypes();
                AvoidOptimize(any, non, nonPattern, nonTypes);

                pooling.ShouldPooled(any, nonPattern);
                pooling.ShouldNotPooled(non, nonTypes);
                PoolingResult = pooling;
            }
        }

        public void Sync()
        {
            var pooling = PoolingResult.New();

            var any = new InterfaceAny();
            var non = new InterfaceNon();
            var nonPattern = new InterfaceNonPattern();
            var nonTypes = new InterfaceNonTypes();
            AvoidOptimize(any, non, nonPattern, nonTypes);

            pooling.ShouldPooled(any, nonPattern);
            pooling.ShouldNotPooled(non, nonTypes);
            PoolingResult = pooling;
        }

        public static void StaticSync()
        {
            var pooling = PoolingResult.New();

            var any = new InterfaceAny();
            var non = new InterfaceNon();
            var nonPattern = new InterfaceNonPattern();
            var nonTypes = new InterfaceNonTypes();
            AvoidOptimize(any, non, nonPattern, nonTypes);

            pooling.ShouldPooled(any, nonPattern);
            pooling.ShouldNotPooled(non, nonTypes);
            PoolingResult = pooling;
        }

        public async Task Async()
        {
            var pooling = PoolingResult.New();

            var any = new InterfaceAny();
            var non = new InterfaceNon();
            await Task.Yield();
            var nonPattern = new InterfaceNonPattern();
            var nonTypes = new InterfaceNonTypes();
            AvoidOptimize(any, non, nonPattern, nonTypes);

            pooling.ShouldPooled(any, nonPattern);
            pooling.ShouldNotPooled(non, nonTypes);
            PoolingResult = pooling;
        }

        public static async Task StaticAsync()
        {
            var pooling = PoolingResult.New();

            var any = new InterfaceAny();
            var non = new InterfaceNon();
            await Task.Yield();
            var nonPattern = new InterfaceNonPattern();
            var nonTypes = new InterfaceNonTypes();
            AvoidOptimize(any, non, nonPattern, nonTypes);

            pooling.ShouldPooled(any, nonPattern);
            pooling.ShouldNotPooled(non, nonTypes);
            PoolingResult = pooling;
        }

        public static IEnumerable<object?> StaticIteraor()
        {
            var pooling = PoolingResult.New();

            var any = new InterfaceAny();
            yield return null;
            var non = new InterfaceNon();
            var nonPattern = new InterfaceNonPattern();
            yield return null;
            var nonTypes = new InterfaceNonTypes();
            AvoidOptimize(any, non, nonPattern, nonTypes);

            pooling.ShouldPooled(any, nonPattern);
            pooling.ShouldNotPooled(non, nonTypes);
            PoolingResult = pooling;
        }

        public IEnumerable<object?> Iteraor()
        {
            var pooling = PoolingResult.New();

            var any = new InterfaceAny();
            yield return null;
            var non = new InterfaceNon();
            var nonPattern = new InterfaceNonPattern();
            yield return null;
            var nonTypes = new InterfaceNonTypes();
            AvoidOptimize(any, non, nonPattern, nonTypes);

            pooling.ShouldPooled(any, nonPattern);
            pooling.ShouldNotPooled(non, nonTypes);
            PoolingResult = pooling;
        }

        public static async IAsyncEnumerable<object?> StaticAsyncIteraor()
        {
            var pooling = PoolingResult.New();

            var any = new InterfaceAny();
            yield return null;
            var non = new InterfaceNon();
            var nonPattern = new InterfaceNonPattern();
            yield return null;
            var nonTypes = new InterfaceNonTypes();
            AvoidOptimize(any, non, nonPattern, nonTypes);

            await Task.Yield();

            pooling.ShouldPooled(any, nonPattern);
            pooling.ShouldNotPooled(non, nonTypes);
            PoolingResult = pooling;
        }

        public static async IAsyncEnumerable<object?> AsyncIteraor()
        {
            var pooling = PoolingResult.New();

            var any = new InterfaceAny();
            yield return null;
            var non = new InterfaceNon();
            var nonPattern = new InterfaceNonPattern();
            yield return null;
            var nonTypes = new InterfaceNonTypes();
            AvoidOptimize(any, non, nonPattern, nonTypes);

            await Task.Yield();

            pooling.ShouldPooled(any, nonPattern);
            pooling.ShouldNotPooled(non, nonTypes);
            PoolingResult = pooling;
        }
    }
}
