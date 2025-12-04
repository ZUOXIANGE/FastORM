using System.ComponentModel.DataAnnotations.Schema;

namespace FastORM.FunctionalTests.Entities;

[Table("Nullables")]
public class NullableEntity
{
    public int Id { get; set; }
    public int? IntVal { get; set; }
    public bool? BoolVal { get; set; }
    public DateTime? DateVal { get; set; }
    public string? StringVal { get; set; }
}
