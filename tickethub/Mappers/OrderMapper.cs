using Riok.Mapperly.Abstractions;
using tickethub.Dtos.Order;
using tickethub.Models;

namespace tickethub.Mappers;

[Mapper]
public partial class OrderMapper
{
    [MapperIgnoreTarget("Id")]
    [MapperIgnoreTarget("UnitPrice")]
    [MapperIgnoreTarget("ConcertId")]
    [MapperIgnoreTarget("Concert")]
    public partial Order ToEntity(CreateOrderRequest request);

    public OrderResponse ToResponse(Order order)
    {
        return new OrderResponse(
            order.Id,
            order.CustomerEmail,
            order.Qty,
            order.UnitPrice,
            order.Qty * order.UnitPrice,
            order.ConcertId
        );
    }
}