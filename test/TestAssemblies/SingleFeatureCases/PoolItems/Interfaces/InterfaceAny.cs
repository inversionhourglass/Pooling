namespace SingleFeatureCases.PoolItems.Interfaces
{
    /// <summary>
    /// 不排除任何类型，所有new该类型的地方都会替换为从池中获取
    /// </summary>
    public class InterfaceAny : PoolItem
    {
    }
}
