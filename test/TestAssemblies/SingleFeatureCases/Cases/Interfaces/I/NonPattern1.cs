using Pooling;
using SingleFeatureCases.PoolItems.Interfaces;
using System.Threading.Tasks;

namespace SingleFeatureCases.Cases.Interfaces.I
{
    public class NonPattern1 : Stateful<NonPattern1>
    {
        [NonPooled]
        public static object? Activator { get; }

        static NonPattern1()
        {
            var pooling = PoolingResult.New();

            var any = new InterfaceAny();
            var non = new InterfaceNon();
            var nonPattern = new InterfaceNonPattern();
            var nonTypes = new InterfaceNonTypes();
            AvoidOptimize(any, non, nonPattern, nonTypes);

            pooling.ShouldPooled(any, nonTypes);
            pooling.ShouldNotPooled(non, nonPattern);
            PoolingResult = pooling;
        }

        public NonPattern1()
        {
            var pooling = PoolingResult.New();

            var any = new InterfaceAny();
            var non = new InterfaceNon();
            var nonPattern = new InterfaceNonPattern();
            var nonTypes = new InterfaceNonTypes();
            AvoidOptimize(any, non, nonPattern, nonTypes);

            pooling.ShouldPooled(any, nonTypes);
            pooling.ShouldNotPooled(non, nonPattern);
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

                pooling.ShouldPooled(any, nonTypes);
                pooling.ShouldNotPooled(non, nonPattern);
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

                pooling.ShouldPooled(any, nonTypes);
                pooling.ShouldNotPooled(non, nonPattern);
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

                pooling.ShouldPooled(any, nonTypes);
                pooling.ShouldNotPooled(non, nonPattern);
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

                pooling.ShouldPooled(any, nonTypes);
                pooling.ShouldNotPooled(non, nonPattern);
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

            pooling.ShouldPooled(any, nonTypes);
            pooling.ShouldNotPooled(non, nonPattern);
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

            pooling.ShouldPooled(any, nonTypes);
            pooling.ShouldNotPooled(non, nonPattern);
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

            pooling.ShouldPooled(any, nonTypes);
            pooling.ShouldNotPooled(non, nonPattern);
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

            pooling.ShouldPooled(any, nonTypes);
            pooling.ShouldNotPooled(non, nonPattern);
            PoolingResult = pooling;
        }
    }
}
