using System.ComponentModel.DataAnnotations.Schema;

namespace FastORM.FunctionalTests.Entities;

[Table("Products")]
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public Guid CategoryId { get; set; }
}
