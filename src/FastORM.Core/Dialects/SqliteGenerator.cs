namespace FastORM;

public class SqliteGenerator : SqlGenerator
{
    public override string Quote(string identifier)
    {
        return "\"" + identifier + "\"";
    }
}