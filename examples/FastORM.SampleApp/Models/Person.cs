using System.ComponentModel.DataAnnotations.Schema;

namespace FastORM.SampleApp.Models;

[Table("People")]
public sealed class Person
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Age { get; set; }
    
}
