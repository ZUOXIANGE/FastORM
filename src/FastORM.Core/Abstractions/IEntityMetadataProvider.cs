namespace FastORM;

/// <summary>
/// 实体元数据提供者接口。
/// </summary>
public interface IEntityMetadataProvider
{
    /// <summary>
    /// 获取指定实体类型的元数据。
    /// </summary>
    /// <typeparam name="T">实体类型。</typeparam>
    /// <returns>实体元数据。</returns>
    IEntityMetadata<T> Get<T>();
}
