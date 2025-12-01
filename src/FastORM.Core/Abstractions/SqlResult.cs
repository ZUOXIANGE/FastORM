namespace FastORM;

public class SqlResult
{
    public string Sql { get; set; } = "";

    public Dictionary<string, object> Parameters { get; set; } = new();
}
