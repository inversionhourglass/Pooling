using Pooling;
using SingleFeatureCases.PoolItems.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SingleFeatureCases.Cases.Interfaces.I
{
    public partial class Other1 : Stateful<Other1>
    {
        [NonPooled]
        public static object? Activator { get; }

        static Other1()
        {
            var pooling = PoolingResult.New();

            var any = new InterfaceAny();
            var non = new InterfaceNon();
            var nonPattern = new InterfaceNonPattern();
            var nonTypes = new InterfaceNonTypes();
            AvoidOptimize(any, non, nonPattern, nonTypes);

            SetPoolingResult(pooling, any, non, nonPattern, nonTypes);
        }

        public Other1()
        {
            var pooling = PoolingResult.New();

            var any = new InterfaceAny();
            var non = new InterfaceNon();
            var nonPattern = new InterfaceNonPattern();
            var nonTypes = new InterfaceNonTypes();
            AvoidOptimize(any, non, nonPattern, nonTypes);

            SetPoolingResult(pooling, any, non, nonPattern, nonTypes);
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

                SetPoolingResult(pooling, any, non, nonPattern, nonTypes);

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

                SetPoolingResult(pooling, any, non, nonPattern, nonTypes);
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

                SetPoolingResult(pooling, any, non, nonPattern, nonTypes);

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

                SetPoolingResult(pooling, any, non, nonPattern, nonTypes);
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

            SetPoolingResult(pooling, any, non, nonPattern, nonTypes);
        }

        public static void StaticSync()
        {
            var pooling = PoolingResult.New();

            var any = new InterfaceAny();
            var non = new InterfaceNon();
            var nonPattern = new InterfaceNonPattern();
            var nonTypes = new InterfaceNonTypes();
            AvoidOptimize(any, non, nonPattern, nonTypes);

            SetPoolingResult(pooling, any, non, nonPattern, nonTypes);
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

            SetPoolingResult(pooling, any, non, nonPattern, nonTypes);
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

            SetPoolingResult(pooling, any, non, nonPattern, nonTypes);
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

            SetPoolingResult(pooling, any, non, nonPattern, nonTypes);
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

            SetPoolingResult(pooling, any, non, nonPattern, nonTypes);
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

            SetPoolingResult(pooling, any, non, nonPattern, nonTypes);
        }

        public async IAsyncEnumerable<object?> AsyncIteraor()
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

            SetPoolingResult(pooling, any, non, nonPattern, nonTypes);
        }
    }
}
