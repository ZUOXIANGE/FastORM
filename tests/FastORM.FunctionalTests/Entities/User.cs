using System.ComponentModel.DataAnnotations.Schema;

namespace FastORM.FunctionalTests.Entities;

[Table("Users")]
public sealed class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Age { get; set; }
}
