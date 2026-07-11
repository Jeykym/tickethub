using tickethub.Mappers;
using tickethub.Models;

namespace tickethub.Tests.Orders;

public class OrderMapperTests
{
    private readonly OrderMapper _mapper = new();

    // ---------- ToResponse ----------

    [Fact]
    public void ToResponse_MapsAllFieldsToMatchingResponseProperties()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerEmail = "buyer@example.com",
            Qty = 4,
            UnitPrice = 25m,
            ConcertId = 42
        };

        var response = _mapper.ToResponse(order);

        Assert.Equal(order.Id, response.Id);
        Assert.Equal(order.CustomerEmail, response.Email);
        Assert.Equal(order.Qty, response.Qty);
        Assert.Equal(order.ConcertId, response.ConcertId);
    }

    [Theory]
    [InlineData(1, 25, 25)]
    [InlineData(3, 25, 75)]
    [InlineData(5, 19.99, 99.95)]
    [InlineData(0, 25, 0)]
    public void ToResponse_ComputesTotalPriceAsQtyTimesUnitPrice(
        int qty, decimal unitPrice, decimal expectedTotal)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerEmail = "buyer@example.com",
            Qty = qty,
            UnitPrice = unitPrice,
            ConcertId = 1
        };

        var response = _mapper.ToResponse(order);

        Assert.Equal(expectedTotal, response.TotalPrice);
    }

    [Fact]
    public void ToResponse_PreservesUnitPricePrecision()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerEmail = "buyer@example.com",
            Qty = 1,
            UnitPrice = 19.99m,
            ConcertId = 1
        };

        var response = _mapper.ToResponse(order);

        Assert.Equal(19.99m, response.UnitPrice);
    }
}