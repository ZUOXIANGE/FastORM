using System.Data.Common;

namespace FastORM.SampleApp;

public sealed class MyDbContext : FastDbContext
{
    public MyDbContext(DbConnection connection, SqlDialect dialect) : base(connection, dialect) { }

    public IQueryable<Models.Person> Person => new FastOrmQueryable<Models.Person>(this, "People");
    public IQueryable<Models.Order> Orders => new FastOrmQueryable<Models.Order>(this, "Orders");
}
