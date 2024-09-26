using System;
using System.Text;

namespace Lib
{
    public class StringBuilderCases
    {
        private static StringBuilder _Builder;
        private StringBuilder _builder;

        public StringBuilder Builder { get; set; }

        // 方法1：将StringBuilder实例存入实例字段_builder
        public void StoreInInstanceField()
        {
            _builder = new StringBuilder();
            _builder.Append("Stored in instance field");
            // 其他操作
            Console.WriteLine("Instance field operation completed.");
        }

        // 方法2：将StringBuilder实例存入静态字段_Builder
        public static void StoreInStaticField()
        {
            _Builder = new StringBuilder();
            _Builder.Append("Stored in static field");
            // 其他操作
            Console.WriteLine("Static field operation completed.");
        }

        // 方法3：将StringBuilder实例存入属性Builder
        public void StoreInProperty()
        {
            Builder = new StringBuilder();
            Builder.Append("Stored in property");
            // 其他操作
            Console.WriteLine("Property operation completed.");
        }

        // 方法4：仅作为方法变量使用
        public void UseAsMethodVariable()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Used as method variable");
            // 其他操作
            Console.WriteLine(sb.ToString());
        }

        // 方法5：实例保存在变量中，在中间进行很多操作后又存入了字段
        public void StoreInVariableThenField()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Initial content. ");
            // 其他操作
            Console.WriteLine("Performing intermediate operations...");
            sb.Append("More content. ");
            // 其他操作
            Console.WriteLine("Performing more intermediate operations...");
            sb.Append("Even more content. ");
            _builder = sb;
            Console.WriteLine("Stored in instance field after operations.");
        }

        // 方法6：实例保存在变量中，在中间进行很多操作后又存入了属性
        public void StoreInVariableThenProperty()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Initial content. ");
            // 其他操作
            Console.WriteLine("Performing intermediate operations...");
            sb.Append("More content. ");
            // 其他操作
            Console.WriteLine("Performing more intermediate operations...");
            sb.Append("Even more content. ");
            Builder = sb;
            Console.WriteLine("Stored in property after operations.");
        }

        // 方法7：实例保存在变量中，在中间进行很多操作后又存入了静态字段
        public static void StoreInVariableThenStaticField()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Initial content. ");
            // 其他操作
            Console.WriteLine("Performing intermediate operations...");
            sb.Append("More content. ");
            // 其他操作
            Console.WriteLine("Performing more intermediate operations...");
            sb.Append("Even more content. ");
            _Builder = sb;
            Console.WriteLine("Stored in static field after operations.");
        }

        // 方法8：复杂操作，涉及多个StringBuilder实例
        public void ComplexOperations()
        {
            StringBuilder sb1 = new StringBuilder();
            sb1.Append("First StringBuilder. ");
            // 其他操作
            Console.WriteLine("First StringBuilder operations...");

            StringBuilder sb2 = new StringBuilder();
            sb2.Append("Second StringBuilder. ");
            // 其他操作
            Console.WriteLine("Second StringBuilder operations...");

            sb1.Append("Appending more to first. ");
            _builder = sb1;
            Console.WriteLine("Stored first StringBuilder in instance field.");

            sb2.Append("Appending more to second. ");
            Builder = sb2;
            Console.WriteLine("Stored second StringBuilder in property.");
        }
    }
}
