# 快速入门

本指南将帮助你快速在 .NET 项目中集成 FastORM。

## 1. 安装

确保你的项目是 .NET 8.0 或更高版本。

目前 FastORM 作为一个源码生成器库，通常通过引用项目或 NuGet 包（未来发布）来使用。

## 2. 定义实体

FastORM 使用 `System.ComponentModel.DataAnnotations.Schema.TableAttribute` 来映射数据库表。

```csharp
using System.ComponentModel.DataAnnotations.Schema;

[Table("Products")]
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
}
```

> **注意**：目前 FastORM 假设实体属性名称与数据库列名一致。

## 3. 创建 DbContext

创建一个继承自 `FastORM.FastDbContext` 的类，并为每个表定义一个 `IQueryable<T>` 属性。

```csharp
using FastORM;
using System.Linq;
using System.Data.Common;

public class AppDbContext : FastDbContext
{
    public AppDbContext(DbConnection connection, SqlDialect dialect) 
        : base(connection, dialect) 
    {
    }

    // 定义数据集
    public IQueryable<Product> Products => new FastOrmQueryable<Product>(this, "Products");
}
```

## 4. 初始化和使用

```csharp
using Microsoft.Data.Sqlite; // 或其他数据库驱动
using FastORM;

// 1. 创建数据库连接
using var connection = new SqliteConnection("Data Source=app.db");
await connection.OpenAsync();

// 2. 初始化 Context
// 第二个参数指定数据库方言：SqlServer, MySql, PostgreSql, Sqlite
var context = new AppDbContext(connection, SqlDialect.Sqlite);

// 3. 开始使用
await context.InsertAsync(new Product { Name = "Laptop", Price = 1200 });

var cheapProducts = await context.Products
    .Where(p => p.Price < 500)
    .ToListAsync();
```

## 5. CRUD 操作指南

FastORM 提供了丰富且高性能的 CRUD 操作支持，所有操作的代码均在编译时生成。

### 插入 (Insert)

#### 单条插入

```csharp
var user = new User { Name = "Alice", Age = 30 };
int rowsAffected = await context.InsertAsync(user);
```

#### 批量插入

FastORM 对批量插入进行了优化，生成的 SQL 会一次性插入多条记录。

```csharp
var users = new[]
{
    new User { Name = "Bob", Age = 25 },
    new User { Name = "Charlie", Age = 35 }
};
int rowsAffected = await context.InsertAsync(users);
```

### 更新 (Update)

#### 实体更新

更新单个实体的所有字段（主键除外）。

```csharp
var user = await context.Users.FirstOrDefaultAsync(u => u.Id == 1);
if (user != null)
{
    user.Age = 31;
    await context.UpdateAsync(user);
}
```

#### 条件更新 (Where Update)

这是 FastORM 的一大亮点。你可以直接使用 LINQ `Where` 子句配合 `UpdateAsync` 来批量更新符合条件的记录，无需先查询出来。生成的 SQL 是高效的 `UPDATE ... SET ... WHERE ...`。

**语法**：
```csharp
context.Table.Where(predicate).Update(setter_lambda)
```

**示例**：
将所有年龄超过 100 的用户的年龄重置为 100。

```csharp
int affected = await context.Users
    .Where(u => u.Age > 100)
    .UpdateAsync(u => u.Age = 100);
```

> **注意**：`UpdateAsync` 的参数必须是一个 **static lambda**（如果支持）或普通 lambda，用于指定要更新的字段和值。

### 删除 (Delete)

#### 实体删除

删除指定的实体（根据主键）。

```csharp
var user = new User { Id = 1 }; // 仅需主键
await context.DeleteAsync(user);
```

#### 批量实体删除

```csharp
var usersToDelete = new[] { new User { Id = 1 }, new User { Id = 2 } };
await context.DeleteAsync(usersToDelete);
```

#### 条件删除 (Where Delete)

类似于条件更新，你可以直接删除符合条件的记录。

**示例**：
删除所有名字为 "Unknown" 的用户。

```csharp
int affected = await context.Users
    .Where(u => u.Name == "Unknown")
    .DeleteAsync();
```

生成的 SQL 类似于：
```sql
DELETE FROM Users WHERE Name = 'Unknown'
```
