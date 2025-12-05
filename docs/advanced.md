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

## 表结构管理 (Schema Management)

FastORM 提供了基本的表结构管理功能，支持通过代码创建和删除数据库表。

### 1. 创建表

```csharp
// 自动生成 CREATE TABLE 语句并执行
await context.CreateTableAsync<User>();
```

### 2. 删除表

```csharp
// 自动生成 DROP TABLE 语句并执行
await context.DropTableAsync<User>();
```

## 特性支持 (Attribute Support)

FastORM 支持多种特性来定制实体与数据库表的映射。

### 标准特性 (System.ComponentModel.DataAnnotations.Schema)

*   **[Table("Name")]**: 指定映射的表名。
*   **[Column("Name", TypeName = "varchar(50)")]**: 指定列名和数据库类型。
*   **[NotMapped]**: 忽略该属性，不映射到数据库列。
*   **[DatabaseGenerated(DatabaseGeneratedOption.Identity)]**: 标识自增主键（默认 int/long 主键为自增）。

### 验证特性 (System.ComponentModel.DataAnnotations)

*   **[Key]**: 标识主键。
*   **[Required]**: 标识列为 NOT NULL（值类型默认为 NOT NULL）。
*   **[MaxLength(100)]** / **[StringLength(100)]**: 指定字符串列的最大长度。

### FastORM 扩展特性

为了提供更丰富的数据库支持，FastORM 提供了一些扩展特性：

*   **[FastORM.DefaultValueSql("CURRENT_TIMESTAMP")]**: 指定列的默认值 SQL 表达式。
*   **[FastORM.Precision(18, 2)]**: 指定 decimal 类型的精度和小数位。
*   **[FastORM.Index(nameof(Prop1), nameof(Prop2), IsUnique = true)]**: 在类级别定义索引。

#### 示例

```csharp
using FastORM;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("users")]
[FastORM.Index(nameof(Email), IsUnique = true)] // 定义唯一索引
public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = "";

    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = "";

    [Column("created_at")]
    [DefaultValueSql("CURRENT_TIMESTAMP")] // 默认值
    public DateTime CreatedAt { get; set; }
    
    [Precision(18, 4)]
    public decimal Balance { get; set; }

    [NotMapped] // 不映射到数据库
    public string TempData { get; set; }
}
```
