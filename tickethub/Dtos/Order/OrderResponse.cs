namespace tickethub.Dtos.Order;

public record OrderResponse(
    Guid Id,
    string Email,
    int Qty,
    decimal UnitPrice,
    decimal TotalPrice,
    int ConcertId
);