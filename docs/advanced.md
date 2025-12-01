# 高级功能

## 事务管理

FastORM 提供了简单的事务管理 API。

```csharp
// 开启事务
using var transaction = await context.BeginTransactionAsync();

try
{
    await context.InsertAsync(new User { Name = "A" });
    await context.InsertAsync(new User { Name = "B" });
    
    // 提交事务
    await transaction.CommitAsync();
}
catch
{
    // 回滚事务
    await transaction.RollbackAsync();
    throw;
}
```

## SQL 日志

你可以通过设置 `SqlLogger` 属性来查看生成的 SQL 语句，这对于调试非常有用。

```csharp
context.SqlLogger = sql => Console.WriteLine($"[SQL] {sql}");
```

## AOT 支持

FastORM 的核心设计目标之一就是支持 Native AOT。由于完全避免了运行时反射（Runtime Reflection）和动态代码生成（Dynamic Code Generation，如 `System.Reflection.Emit`），FastORM 生成的代码是完全静态的，非常适合 AOT 编译。

## 数据库方言

FastORM 通过 `SqlDialect` 枚举支持多种数据库。在创建 Context 时指定：

*   `SqlDialect.SqlServer`
*   `SqlDialect.MySql`
*   `SqlDialect.PostgreSql`
*   `SqlDialect.Sqlite`

框架会自动处理不同数据库之间的 SQL 语法差异（如分页语法、标识符引用符等）。
