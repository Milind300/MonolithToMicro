
using System.ComponentModel.DataAnnotations;

namespace MonolithToMicro.Order.Api.Models;

public sealed class CreateOrderRequest
{
    [Required]
    public Guid CustomerId { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }
}
