using Cecil.AspectN;
using Cecil.AspectN.Matchers;
using System;
using System.Linq;

namespace Pooling.Fody
{
    /**
     * <Pooling enabled="true" composite-accessibility="false">
     *   <Inspects>
     *     <Inspect>any_aspectn_pattern</Inspect>
     *     <Inspect>any_aspectn_pattern</Inspect>
     *   </Inspects>
     *   <NotInspects>
     *     <NotInspect>any_aspectn_pattern</NotInspect>
     *     <NotInspect>any_aspectn_pattern</NotInspect>
     *   </NotInspects>
     *   <Items>
     *     <Item pattern="method_name_pattern" stateless="type_pattern" inspect="any_aspectn_pattern" not-inspect="any_aspectn_pattern" />
     *     <Item pattern="method_name_pattern" stateless="type_pattern" inspect="any_aspectn_pattern" not-inspect="any_aspectn_pattern" />
     *   </Items>
     * </Pooling>
     */

    /// <summary>
    /// </summary>
    public class Config(string enabled, string compositeAccessibility, string[] inspects, string[] notInspects, Config.Item[] items)
    {
        /// <summary>
        /// enabled. 是否启用Pooling
        /// </summary>
        public bool Enabled { get; } = "true".Equals(enabled, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// composite-accessibility. 是否使用组合可访问性
        /// </summary>
        public bool CompositeAccessibility { get; } = "true".Equals(compositeAccessibility, StringComparison.OrdinalIgnoreCase);

        public IMatcher[] Inspects { get; } = inspects.Select(x => PatternParser.Parse(x).Cached()).ToArray();

        public IMatcher[] NotInspects { get; } = notInspects.Select(x => PatternParser.Parse(x).Cached()).ToArray();

        public Item[] Items { get; } = items;

        public class Item(string? pattern, string? stateless, string? inspect, string? notInspect)
        {
            /// <summary>
            /// 池化对象表达式。该表达式格式固定为`method()`格式，其中类型部分匹配池化对象，方法部分匹配重置方法
            /// </summary>
            /// <remarks>
            /// 由于固定为`method()`格式，所以省略`method()`符号本身，直接编写表达式主体。该表达式与<see cref="Stateless"/>二选一，<see cref="Pattern"/>具有更高优先级
            /// </remarks>
            public IMatcher? Pattern { get; } = string.IsNullOrEmpty(pattern) ? null : new MethodMatcher($"* {pattern}()").Cached();

            /// <summary>
            /// 无状态池化对象表达式。池化对象本身无状态，在回收时不需要重置
            /// </summary>
            /// <remarks>
            /// 表达式格式为类型匹配格式，直接编写表达式主体。该表达式与<see cref="Pattern"/>二选一，<see cref="Pattern"/>具有更高优先级
            /// </remarks>
            public ITypeMatcher? Stateless { get; } = string.IsNullOrEmpty(stateless) ? null : new TypeMatcher(stateless!).Cached();

            /// <summary>
            /// 池化应用目标表达式。匹配哪些方法/属性/构造方法里需要进行对当前池化对象进行检查，发现匹配<see cref="Pattern"/>或<see cref="Stateless"/>的初始化操作时，进行池化操作替换
            /// </summary>
            /// <remarks>
            /// 该表达式缺省时表示匹配当前程序集的所有方法（包含属性和构造方法），表达式格式可用AspectN方法匹配规则中的任意一种或多种的组合
            /// </remarks>
            public IMatcher? Inspect { get; } = string.IsNullOrEmpty(inspect) ? null : PatternParser.Parse(inspect!).Cached();

            /// <summary>
            /// 排除的池化应用目标表达式。匹配哪些方法/属性/构造方法不需要对当前池化对象进行检查
            /// </summary>
            /// <remarks>
            /// 表达式格式可用AspectN方法匹配规则中的任意一种或多种的组合
            /// </remarks>
            public IMatcher? NotInspect { get; } = string.IsNullOrEmpty(notInspect) ? null : PatternParser.Parse(notInspect!).Cached();
        }
    }
}
