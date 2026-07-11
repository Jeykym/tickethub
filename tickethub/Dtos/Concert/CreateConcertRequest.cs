using System.ComponentModel.DataAnnotations;

namespace tickethub.Dtos.Concert;

public sealed record CreateConcertRequest
{
    [Required]
    [MaxLength(50, ErrorMessage = "Title can't be longer than 50 characters")]
    [MinLength(1, ErrorMessage = "Title must be at least 1 character long")]
    public required string Title { get; init; }

    public required DateTime Start { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than or equal to 1")]
    public required int MaxCapacity { get; init; }

    [Range(0, double.MaxValue, ErrorMessage = "TicketPrice must be greater than or equal to 0")]
    public required decimal TicketPrice { get; init; }
}