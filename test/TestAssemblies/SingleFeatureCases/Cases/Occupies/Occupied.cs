using SingleFeatureCases.PoolItems.Interfaces;
using System.Threading.Tasks;

namespace SingleFeatureCases.Cases.Occupies
{
    public class Occupied : Stateful<Occupied>
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private static InterfaceAny _Any;

        public static InterfaceAny StaticAny { get; set; }

        private InterfaceAny _any;

        public InterfaceAny Any { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public void Sync()
        {
            var pooling = PoolingResult.New();

            var any1 = new InterfaceAny();
            var any2 = new InterfaceAny();
            var any3 = new InterfaceAny();
            var any4 = new InterfaceAny();
            var any5 = new InterfaceAny();
            AvoidOptimize(any1, any2, any3, any4, any5);

            StaticAny = any1;
            _Any = any3;
            Any = any4;
            _any = any5;

            pooling.ShouldPooled(any2);
            pooling.ShouldNotPooled(any1, any3, any4, any5);
            PoolingResult = pooling;
        }

        public async Task Async()
        {
            var pooling = PoolingResult.New();

            var any1 = new InterfaceAny();
            await Task.Yield();
            var any2 = new InterfaceAny();
            await Task.Yield();
            var any3 = new InterfaceAny();
            var any4 = new InterfaceAny();
            await Task.Yield();
            var any5 = new InterfaceAny();
            await Task.Yield();
            AvoidOptimize(any1, any2, any3, any4, any5);

            StaticAny = any1;
            _Any = any3;
            Any = any4;
            _any = any5;

            pooling.ShouldPooled(any2);
            pooling.ShouldNotPooled(any1, any3, any4, any5);
            PoolingResult = pooling;
        }
    }
}
