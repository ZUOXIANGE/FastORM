using System.ComponentModel.DataAnnotations.Schema;

namespace FastORM.IntegrationTests.Entities;

[Table("items")]
public sealed class Item
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
}
