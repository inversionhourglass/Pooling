using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Pooling.Fody
{
    internal class PoolItem(Instruction instruction, TypeReference itemTypeRef, MethodDefinition? resetMethodDef = null)
    {
        /// <summary>
        /// 池化对象发现阶段，该属性用于保存newobj操作；
        /// 发现阶段结束后的池化操作阶段，该属性用于保存ldloc操作；
        /// 在池化对象回收阶段，使用上一阶段的ldloc加载池化对象并交还给对象池
        /// </summary>
        public Instruction Instruction { get; set; } = instruction;

        public TypeReference ItemTypeRef { get; } = itemTypeRef;

        public MethodDefinition? ResetMethodDef { get; } = resetMethodDef;
    }
}
