using System.Linq;
using FastORM.IntegrationTests.Entities;

namespace FastORM.IntegrationTests.Contexts;

public sealed class IntegrationTestDbContext : FastDbContext
{
    public IntegrationTestDbContext(System.Data.Common.DbConnection connection, SqlDialect dialect) : base(connection, dialect) { }
    
    // Mapped to 'users' table
    public IQueryable<User> Users => new FastOrmQueryable<User>(this, "users");
    
    // Mapped to 'orders' table
    public IQueryable<Order> Orders => new FastOrmQueryable<Order>(this, "orders");
    
    // Mapped to 'items' table
    public IQueryable<Item> Items => new FastOrmQueryable<Item>(this, "items");

    // Mapped to 'supported_types' table
    public IQueryable<SupportedTypes> SupportedTypes => new FastOrmQueryable<SupportedTypes>(this, "supported_types");
}
