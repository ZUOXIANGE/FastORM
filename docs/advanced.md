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

FastORM 的核心设计目标之一就是支持 Native AOT。由于核心逻辑避免了运行时反射（Runtime Reflection）和动态代码生成（Dynamic Code Generation），FastORM 生成的代码是静态的，非常适合 AOT 编译。

## 混合编译模式 (Hybrid Compilation)

为了兼顾性能与灵活性，FastORM 采用独特的混合编译模式：

1.  **编译时 (Compile-Time)**：
    *   解析数据库实体元数据。
    *   生成 SQL 骨架（SELECT, FROM, JOIN 等）。
    *   生成结果集映射代码 (DataReader -> Entity)。

2.  **运行时 (Runtime)**：
    *   解析动态的 `WHERE` 条件表达式。
    *   提取运行时变量值并绑定为 SQL 参数。
    *   处理 `IN` 子句的动态集合。

这种模式使得 FastORM 既拥有手写 ADO.NET 级别的性能，又具备 LINQ 的动态表达能力。

## 数据库方言

FastORM 通过 `SqlDialect` 枚举支持多种数据库。在创建 Context 时指定：

*   `SqlDialect.SqlServer`
*   `SqlDialect.MySql`
*   `SqlDialect.PostgreSql`
*   `SqlDialect.Sqlite`

框架会自动处理不同数据库之间的 SQL 语法差异（如分页语法、标识符引用符等）。
