namespace FastORM;

public class MySqlGenerator : SqlGenerator
{
    public override string Quote(string identifier)
    {
        return "`" + identifier + "`";
    }
}