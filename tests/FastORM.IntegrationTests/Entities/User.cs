using System.ComponentModel.DataAnnotations.Schema;

namespace FastORM.IntegrationTests.Entities;

[Table("users")]
public sealed class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Age { get; set; }
}
