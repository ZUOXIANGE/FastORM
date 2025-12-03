using System.ComponentModel.DataAnnotations.Schema;

namespace FastORM.IntegrationTests.Entities;

[Table("orders")]
public sealed class Order
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal Amount { get; set; }
}
