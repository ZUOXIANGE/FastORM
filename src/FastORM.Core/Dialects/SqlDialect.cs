namespace FastORM;

/// <summary>
/// 定义支持的 SQL 数据库
/// </summary>
public enum SqlDialect
{
    /// <summary>
    /// Microsoft SQL Server 
    /// </summary>
    SqlServer,

    /// <summary>
    /// PostgreSQL 
    /// </summary>
    PostgreSql,

    /// <summary>
    /// MySQL 
    /// </summary>
    MySql,

    /// <summary>
    /// SQLite 
    /// </summary>
    Sqlite
}