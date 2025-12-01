using System.ComponentModel.DataAnnotations.Schema;

namespace FastORM.SampleApp.Models;

[Table("Orders")]
public sealed class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal Amount { get; set; }
}