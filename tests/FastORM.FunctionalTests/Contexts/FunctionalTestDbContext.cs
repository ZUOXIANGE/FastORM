using FastORM.FunctionalTests.Entities;

namespace FastORM.FunctionalTests.Contexts;

public sealed class FunctionalTestDbContext : FastDbContext
{
    public FunctionalTestDbContext(System.Data.Common.DbConnection connection, SqlDialect dialect) : base(connection, dialect) { }

    public IQueryable<User> Users => new FastOrmQueryable<User>(this, "Users");
    public IQueryable<Order> Orders => new FastOrmQueryable<Order>(this, "Orders");
    public IQueryable<Item> Items => new FastOrmQueryable<Item>(this, "Items");
    public IQueryable<Product> Products => new FastOrmQueryable<Product>(this, "Products");
}
