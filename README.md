# FastORM

[中文](README.md) | [English](README_EN.md)

FastORM 是一个高性能、零运行时反射的 .NET ORM 框架，利用 C# Source Generators 在编译时生成高效的 SQL 和 ADO.NET 执行代码。

## 核心特性

*   **🚀 极致性能**：所有元数据解析、SQL 生成和参数绑定逻辑均在编译时完成，零运行时反射开销，AOT 友好。
*   **🔒 类型安全**：基于标准的 LINQ 语法，编译时检查类型错误。
*   **📦 多数据库支持**：内置支持 SQL Server, MySQL, PostgreSQL, SQLite。
*   **⚡ 异步优先**：全链路 Async/Await 支持，高并发友好。
*   **🛠️ 丰富功能**：
    *   完整的 CRUD 支持（单条、批量）。
    *   支持 Join, GroupBy, Aggregation 等复杂查询。
    *   内置事务管理。

## 快速开始

### 1. 定义实体

```csharp
using System.ComponentModel.DataAnnotations.Schema;

[Table("users")]
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Age { get; set; }
}
```

### 2. 定义上下文

继承 `FastDbContext` 并定义数据集。

```csharp
using FastORM;
using System.Linq;
using System.Data.Common;

public class MyDbContext : FastDbContext
{
    public MyDbContext(DbConnection connection, SqlDialect dialect) 
        : base(connection, dialect) { }

    // 定义数据集
    public IQueryable<User> Users => new FastOrmQueryable<User>(this, "users");
}
```

### 3. 使用示例

```csharp
using Microsoft.Data.Sqlite;
using FastORM;

// 初始化连接和上下文
using var connection = new SqliteConnection("Data Source=:memory:");
await connection.OpenAsync();

// 建表
using (var command = connection.CreateCommand())
{
    command.CommandText = "CREATE TABLE users (Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER)";
    await command.ExecuteNonQueryAsync();
}

var ctx = new MyDbContext(connection, SqlDialect.Sqlite);

// 1. 插入 (Insert)
await ctx.InsertAsync(new User { Id = 1, Name = "Alice", Age = 30 });

// 2. 查询 (Query)
var user = await ctx.Users.Where(u => u.Id == 1).FirstOrDefaultAsync();

// 3. 更新 (Update) - 实体更新
if (user != null)
{
    user.Age = 31;
    await ctx.UpdateAsync(user);
}

// 4. 批量操作 (Bulk)
var users = new[] 
{ 
    new User { Id = 2, Name = "Bob", Age = 25 }, 
    new User { Id = 3, Name = "Carol", Age = 35 },
    new User { Id = 4, Name = "Jackson", Age = 106 },
    new User { Id = 5, Name = "Unknown", Age = 0 }
};
await ctx.InsertAsync(users);

// 5. 条件更新 (Where Update) - 高效！
// 将所有年龄大于 100 的用户年龄重置为 100
await ctx.Users
    .Where(u => u.Age > 100)
    .UpdateAsync(u => u.Age = 100);

// 6. 条件删除 (Where Delete) - 高效！
// 删除所有名字为 "Unknown" 的用户
await ctx.Users
    .Where(u => u.Name == "Unknown")
    .DeleteAsync();
```

## 文档

更多详细文档请参阅 `docs` 目录：

*   [快速入门](docs/getting-started.md)
*   [查询功能指南](docs/querying.md)
*   [高级功能](docs/advanced.md)

## 贡献

欢迎提交 Issue 和 PR！
