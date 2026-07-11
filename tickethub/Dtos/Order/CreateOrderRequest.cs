using System.ComponentModel.DataAnnotations;

namespace tickethub.Dtos.Order;

public record CreateOrderRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(254, ErrorMessage = "CustomerEmail cannot be longer than 254 characters")]
    public required string CustomerEmail { get; init; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than or equal to 1")]
    public int Qty { get; init; }
}