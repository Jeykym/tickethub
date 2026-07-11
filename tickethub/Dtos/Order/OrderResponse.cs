namespace tickethub.Dtos.Order;

public record OrderResponse(
    Guid Id,
    string Email,
    int Qty,
    int UnitPrice,
    decimal TotalPrice,
    int ConcertId
);