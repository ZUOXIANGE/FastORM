using System.Data.Common;

namespace FastORM;

/// <summary>
/// 实体元数据接口，用于描述实体的数据库映射信息。
/// </summary>
/// <typeparam name="T">实体类型。</typeparam>
public interface IEntityMetadata<T>
{
    /// <summary>
    /// 获取表名。
    /// </summary>
    string TableName { get; }

    /// <summary>
    /// 获取所有列名。
    /// </summary>
    string[] Columns { get; }

    /// <summary>
    /// 获取所有属性名（与 Columns 一一对应）。
    /// </summary>
    string[] PropertyNames { get; }

    /// <summary>
    /// 根据属性名获取列名。
    /// </summary>
    /// <param name="propertyName">属性名。</param>
    /// <returns>列名，如果未找到则返回 null。</returns>
    string? GetColumnName(string propertyName);

    /// <summary>
    /// 获取主键列在 Columns 数组中的索引。
    /// </summary>
    int KeyColumnIndex { get; }

    /// <summary>
    /// 获取实体对象中指定列索引的值。
    /// </summary>
    /// <param name="entity">实体对象。</param>
    /// <param name="columnIndex">列索引。</param>
    /// <returns>列的值。</returns>
    object? GetValue(T entity, int columnIndex);

    /// <summary>
    /// 从数据读取器中读取当前行并转换为实体对象。
    /// </summary>
    /// <param name="reader">数据读取器。</param>
    /// <returns>实体对象。</returns>
    T ReadRow(DbDataReader reader);
}
