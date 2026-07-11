namespace tickethub.Dtos.Concert;

public sealed record ConcertResponse(
    int Id,
    string Title,
    DateTime Start,
    int MaxCapacity,
    decimal TicketPrice,
    int TicketsSold
);