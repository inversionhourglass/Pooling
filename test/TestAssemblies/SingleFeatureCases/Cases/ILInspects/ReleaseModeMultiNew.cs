using SingleFeatureCases.PoolItems.Interfaces;
using System;

namespace SingleFeatureCases.Cases.ILInspects
{
    public class ReleaseModeMultiNew
    {
        /// <summary>
        /// 该示例在Release模式下，编译器会将pi变量优化，因此pi产生的对象会暂存在栈顶，直到Console.WriteLine(pi.GetHashCode())直接从栈顶取值。
        /// 这是一种特殊场景，留作测试用。
        /// </summary>
        public static void M()
        {
            var pi = new InterfaceAny();
            var pi1 = new InterfaceAny();
            var pi2 = new InterfaceAny();
            Console.WriteLine(pi.GetHashCode());
            Console.WriteLine(pi1.GetHashCode());
            Console.WriteLine(pi2.GetHashCode());
        }
    }
}
