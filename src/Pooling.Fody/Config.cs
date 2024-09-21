using Cecil.AspectN.Matchers;
using System;
using System.Linq;

namespace Pooling.Fody
{
    /// <summary>
    /// 所有配置的表达式使用英文分号';'作为分隔符，波浪号'~'表示使用try..finally..强制reset
    /// </summary>
    public class Config(string enabled, string compositeAccessibility, string inclusiveMethods, string exclusiveMethods, string pooledMethodTypes, string pooledTypes, string nonPooledTypes)
    {
        /// <summary>
        /// enabled. 是否启用Pooling
        /// </summary>
        public bool Enabled { get; } = "true".Equals(enabled, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// composite-accessibility. 是否使用组合可访问性
        /// </summary>
        public bool CompositeAccessibility { get; } = "true".Equals(compositeAccessibility, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// inclusive-methods. 与该表达式匹配的方法的内部会进行IL检查，优先级低于<see cref="ExclusiveMethods"/>
        /// </summary>
        /// <remarks>
        /// 如果当前表达式缺省，则表示匹配所有方法
        /// </remarks>
        public MethodMatcher[] InclusiveMethods { get; } = inclusiveMethods.Split([';'], StringSplitOptions.RemoveEmptyEntries).Select(x => new MethodMatcher(x)).ToArray();

        /// <summary>
        /// exclusive-methods. 与该表达式匹配的方法的内部不会进行IL检查，用于筛选<see cref="InclusiveMethods"/>的结果集
        /// </summary>
        /// <remarks>
        /// 如果当前表达式缺省，则表示不对<see cref="InclusiveMethods"/>的结果集进行筛选
        /// </remarks>
        public MethodMatcher[] ExclusiveMethods { get; } = exclusiveMethods.Split([';'], StringSplitOptions.RemoveEmptyEntries).Select(x => new MethodMatcher(x)).ToArray();

        /// <summary>
        /// pooled-method-types. 自定义reset方法的池化类型，表达式的类型部分与<see cref="PooledTypes"/>作用相同，方法部分用来表示自定义的reset方法
        /// </summary>
        /// <remarks>
        /// 在当前表达式中定义的池化类型不需要到<see cref="PooledTypes"/>中重复定义，当然，重复了也没关系，<see cref="PooledMethodTypes"/>具有更高优先级
        /// </remarks>
        public MethodMatcher[] PooledMethodTypes { get; } = pooledMethodTypes.Split([';'], StringSplitOptions.RemoveEmptyEntries).Select(x => new MethodMatcher(x)).ToArray();

        /// <summary>
        /// pooled-types. 进行池化的类型，在方法内部进行IL检查时，与该表达式匹配的类型将进行池化操作，优先级低于<see cref="NonPooledTypes"/>
        /// </summary>
        /// <remarks>
        /// 如果当前表达式缺省，则表示不匹配任何类型，如果<see cref="PooledMethodTypes"/>也缺省，那么Pooling将不会对任何类型进行池化操作
        /// </remarks>
        public TypeMatcher[] PooledTypes { get; } = pooledTypes.Split([';'], StringSplitOptions.RemoveEmptyEntries).Select(x => new TypeMatcher(x)).ToArray();

        /// <summary>
        /// non-pooled-types. 不进行池化操作的类型。在方法内部进行IL检查时，与该表达式匹配的类型将不进行池化操作，用于筛选<see cref="PooledMethodTypes"/>和<see cref="PooledTypes"/>的结果集
        /// </summary>
        /// <remarks>
        /// 如果当前表达式缺省，则表示不对<see cref="PooledMethodTypes"/>和<see cref="PooledTypes"/>的结果集进行筛选
        /// </remarks>
        public TypeMatcher[] NonPooledTypes { get; } = nonPooledTypes.Split([';'], StringSplitOptions.RemoveEmptyEntries).Select(x => new TypeMatcher(x)).ToArray();
    }
}
