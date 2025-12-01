namespace FastORM;

/// <summary>
/// 实体元数据注册表，用于管理和访问实体元数据。
/// </summary>
public static class EntityMetadataRegistry
{
    static IEntityMetadataProvider? _provider;

    /// <summary>
    /// 设置元数据提供者。
    /// </summary>
    /// <param name="provider">元数据提供者实例。</param>
    public static void SetProvider(IEntityMetadataProvider provider) { _provider = provider; }

    /// <summary>
    /// 获取指定实体类型的元数据。
    /// </summary>
    /// <typeparam name="T">实体类型。</typeparam>
    /// <returns>实体元数据。</returns>
    /// <exception cref="NotSupportedException">如果未初始化元数据提供者。</exception>
    public static IEntityMetadata<T> Get<T>()
    {
        if (_provider is null) throw new NotSupportedException("FastORM metadata provider not initialized");
        return _provider.Get<T>();
    }
}
