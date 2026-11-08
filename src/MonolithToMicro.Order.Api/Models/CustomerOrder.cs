
namespace MonolithToMicro.Order.Api.Models;

public sealed class CustomerOrder
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CustomerId { get; set; }

    // Day 4: Product API integration
    public Guid ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}
