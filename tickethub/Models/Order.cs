using System.ComponentModel.DataAnnotations;

namespace tickethub.Models;

public class Order
{
    [Key]
    public Guid Id { get; set; } = Guid.CreateVersion7();
    
    [Required]
    [EmailAddress(ErrorMessage = "CustomerEmail must be a valid email address")]
    [MaxLength(254, ErrorMessage = "CustomerEmail cannot be longer than 255 characters")]
    public required string CustomerEmail { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than or equal to 1")]
    public int Qty { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "UnitPrice must be greater than or equal to 0")]
    public decimal UnitPrice { get; set; }
    
    public int ConcertId { get; set; }

    public Concert Concert { get; set; } = null!;
}