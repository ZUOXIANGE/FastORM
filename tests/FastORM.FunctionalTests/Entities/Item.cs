using System.ComponentModel.DataAnnotations.Schema;

namespace FastORM.FunctionalTests.Entities;

[Table("Items")]
public sealed class Item
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
}
