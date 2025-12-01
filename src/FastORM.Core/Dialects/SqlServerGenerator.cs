namespace FastORM;

public class SqlServerGenerator : SqlGenerator
{
    public override string Quote(string identifier)
    {
        return "[" + identifier + "]";
    }
}