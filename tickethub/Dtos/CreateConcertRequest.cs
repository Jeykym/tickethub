using System;
using System.ComponentModel.DataAnnotations;

namespace tickethub.Dtos;

public sealed record CreateConcertRequest(
    [property: Required]
    [property: MaxLength(50, ErrorMessage = "Title can't be longer than 50 characters")]
    [property: MinLength(1, ErrorMessage = "Title must be at least 1 character long")]
    string Title,
    
    DateTime Start,
    
    [property: Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than or equal to 1")]
    int MaxCapacity,
    
    [property: Range(0, double.MaxValue, ErrorMessage = "TicketPrice must be greater than or equal to 0")]
    decimal TicketPrice
);