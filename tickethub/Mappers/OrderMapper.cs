using Riok.Mapperly.Abstractions;
using tickethub.Dtos.Concert;
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

    private decimal TotalPrice(Order order)
    {
        return order.UnitPrice * order.Qty;
    }
}