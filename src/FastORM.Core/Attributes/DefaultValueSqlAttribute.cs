using System;

namespace FastORM;

[AttributeUsage(AttributeTargets.Property)]
public class DefaultValueSqlAttribute : Attribute
{
    public string Sql { get; }
    public DefaultValueSqlAttribute(string sql) => Sql = sql;
}
