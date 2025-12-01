namespace FastORM;

public class PostgreSqlGenerator : SqlGenerator
{
    public override string Quote(string identifier)
    {
        return "\"" + identifier.ToLowerInvariant() + "\"";
    }
}