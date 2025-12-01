using System.ComponentModel.DataAnnotations.Schema;

namespace FastORM.FunctionalTests.Entities;

[Table("Items")]
public sealed class Item
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
}
