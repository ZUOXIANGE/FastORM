# FastORM

[中文](README.md) | [English](README_EN.md)

FastORM is a high-performance, zero-runtime reflection .NET ORM framework that uses C# Source Generators to generate efficient SQL and ADO.NET execution code at compile time.

## Core Features

*   **🚀 Extreme Performance**: All metadata parsing, SQL generation, and parameter binding logic are completed at compile time, with zero runtime reflection overhead, making it AOT friendly.
*   **🔒 Type Safety**: Based on standard LINQ syntax, checking for type errors at compile time.
*   **📦 Multi-Database Support**: Built-in support for SQL Server, MySQL, PostgreSQL, SQLite.
*   **⚡ Async First**: Full chain Async/Await support, friendly to high concurrency.
*   **🛠️ Rich Features**:
    *   Complete CRUD support (single, batch).
    *   Support for complex queries like Join, GroupBy, Aggregation, etc.
    *   Built-in transaction management.

## Quick Start

### 1. Define Entity

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

### 2. Define Context

Inherit from `FastDbContext` and define the dataset.

```csharp
using FastORM;
using System.Linq;
using System.Data.Common;

public class MyDbContext : FastDbContext
{
    public MyDbContext(DbConnection connection, SqlDialect dialect) 
        : base(connection, dialect) { }

    // Define dataset
    public IQueryable<User> Users => new FastOrmQueryable<User>(this, "users");
}
```

### 3. Usage Example

```csharp
using Microsoft.Data.Sqlite;
using FastORM;

// Initialize connection and context
using var connection = new SqliteConnection("Data Source=:memory:");
await connection.OpenAsync();

// Create table
using (var command = connection.CreateCommand())
{
    command.CommandText = "CREATE TABLE users (Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER)";
    await command.ExecuteNonQueryAsync();
}

var ctx = new MyDbContext(connection, SqlDialect.Sqlite);

// 1. Insert
await ctx.InsertAsync(new User { Id = 1, Name = "Alice", Age = 30 });

// 2. Query
var user = await ctx.Users.Where(u => u.Id == 1).FirstOrDefaultAsync();

// 3. Update - Entity Update
if (user != null)
{
    user.Age = 31;
    await ctx.UpdateAsync(user);
}

// 4. Bulk Operations
var users = new[] 
{ 
    new User { Id = 2, Name = "Bob", Age = 25 }, 
    new User { Id = 3, Name = "Carol", Age = 35 },
    new User { Id = 4, Name = "Jackson", Age = 106 },
    new User { Id = 5, Name = "Unknown", Age = 0 }
};
await ctx.InsertAsync(users);

// 5. Conditional Update (Where Update) - Efficient!
// Reset age to 100 for all users older than 100
await ctx.Users
    .Where(u => u.Age > 100)
    .UpdateAsync(u => u.Age = 100);

// 6. Conditional Delete (Where Delete) - Efficient!
// Delete all users named "Unknown"
await ctx.Users
    .Where(u => u.Name == "Unknown")
    .DeleteAsync();
```

## Documentation

For more detailed documentation, please refer to the `docs` directory:

*   [Getting Started](docs/getting-started.md)
*   [Querying Guide](docs/querying.md)
*   [Advanced Features](docs/advanced.md)

## Contribution

Issues and PRs are welcome!
