
using System.ComponentModel.DataAnnotations;

namespace MonolithToMicro.Order.Api.Models;

public sealed class UpdateOrderStatusRequest
{
    [Required]
    [RegularExpression(
        "^(Pending|Confirmed|Cancelled)$",
        ErrorMessage = "Status must be Pending, Confirmed or Cancelled.")]
    public string Status { get; set; } = string.Empty;
}
