using System.ComponentModel.DataAnnotations;

namespace tickethub.Models;

public class Concert
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50, ErrorMessage = "Title can't be longer than 50 characters")]
    [MinLength(1, ErrorMessage = "Title must be at least 1 character long")]
    public required string Title { get; set; }
    
    public DateTime Start { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than or equal to 1")]
    public int MaxCapacity { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "TicketPrice must be greater than or equal to 0")]
    public decimal TicketPrice { get; set; }

    [ConcurrencyCheck]
    [Range(0, int.MaxValue, ErrorMessage = "TicketsSold must be greater than or equal to 0")]
    public int TicketsSold { get; set; } = 0;
}