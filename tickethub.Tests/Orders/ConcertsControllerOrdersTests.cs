using System.Data.Common;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using tickethub.Controllers;
using tickethub.Dtos.Concert;
using tickethub.Dtos.Order;
using Xunit.Abstractions;

namespace tickethub.Tests.Orders;

public class ConcertsControllerOrderTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly SqliteConnection _connection;

    public ConcertsControllerOrderTests(WebApplicationFactory<Program> factory)
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }

                var dbConnectionDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbConnection));

                if (dbConnectionDescriptor != null)
                {
                    services.Remove(dbConnectionDescriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlite(_connection));
            });

            builder.UseEnvironment("Development");
        });

        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    private static CreateConcertRequest ValidConcertRequest(
        string title = "Metallica",
        DateTime? start = null,
        int maxCapacity = 5000,
        decimal ticketPrice = 675)
    {
        return new CreateConcertRequest
        {
            Title = title,
            Start = start ?? DateTime.Now,
            MaxCapacity = maxCapacity,
            TicketPrice = ticketPrice
        };
    }

    private static CreateOrderRequest ValidOrderRequest(
        string email = "buyer@example.com",
        int qty = 1)
    {
        return new CreateOrderRequest
        {
            CustomerEmail = email,
            Qty = qty
        };
    }

    private void ClearDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Orders.RemoveRange(db.Orders);
        db.Concerts.RemoveRange(db.Concerts);
        db.SaveChanges();
    }

    private async Task<ConcertResponse> CreateConcert(
        int maxCapacity = 5000,
        decimal ticketPrice = 675)
    {
        var request = ValidConcertRequest(maxCapacity: maxCapacity, ticketPrice: ticketPrice);
        var response = await _client.PostAsJsonAsync("/api/concerts", request);
        var concert = await response.Content.ReadFromJsonAsync<ConcertResponse>();
        Assert.NotNull(concert);
        return concert;
    }

    [Fact]
    public async Task CreateOrder_ValidRequest_Returns201()
    {
        ClearDatabase();
        var concert = await CreateConcert();
        var request = ValidOrderRequest();

        var response = await _client.PostAsJsonAsync($"/api/concerts/{concert.Id}/orders", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_ConcertDoesNotExist_Returns404()
    {
        ClearDatabase();
        var request = ValidOrderRequest();

        var response = await _client.PostAsJsonAsync("/api/concerts/999999/orders", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_QtyNegative_Returns400()
    {
        ClearDatabase();
        var concert = await CreateConcert();
        var request = ValidOrderRequest(qty: -1);

        var response = await _client.PostAsJsonAsync($"/api/concerts/{concert.Id}/orders", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_QtyZero_Returns400()
    {
        ClearDatabase();
        var concert = await CreateConcert();
        var request = ValidOrderRequest(qty: 0);

        var response = await _client.PostAsJsonAsync($"/api/concerts/{concert.Id}/orders", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_QtyOne_Returns201()
    {
        ClearDatabase();
        var concert = await CreateConcert();
        var request = ValidOrderRequest(qty: 1);

        var response = await _client.PostAsJsonAsync($"/api/concerts/{concert.Id}/orders", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_QtyMissing_Returns400()
    {
        ClearDatabase();
        var concert = await CreateConcert();
        var request = new { CustomerEmail = "buyer@example.com" };

        var response = await _client.PostAsJsonAsync($"/api/concerts/{concert.Id}/orders", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_EmailMissingAtSymbol_Returns400()
    {
        ClearDatabase();
        var concert = await CreateConcert();
        var request = ValidOrderRequest(email: "buyerexample.com");

        var response = await _client.PostAsJsonAsync($"/api/concerts/{concert.Id}/orders", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_EmailMissingLocalPart_Returns400()
    {
        ClearDatabase();
        var concert = await CreateConcert();
        var request = ValidOrderRequest(email: "@example.com");

        var response = await _client.PostAsJsonAsync($"/api/concerts/{concert.Id}/orders", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_EmailMissingDomainPart_Returns400()
    {
        ClearDatabase();
        var concert = await CreateConcert();
        var request = ValidOrderRequest(email: "buyer@");

        var response = await _client.PostAsJsonAsync($"/api/concerts/{concert.Id}/orders", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_Email254Characters_Returns201()
    {
        ClearDatabase();
        var concert = await CreateConcert();
        var localPart = new string('a', 242);
        var email = $"{localPart}@example.com";
        var request = ValidOrderRequest(email: email);

        var response = await _client.PostAsJsonAsync($"/api/concerts/{concert.Id}/orders", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_Email255Characters_Returns400()
    {
        ClearDatabase();
        var concert = await CreateConcert();
        var localPart = new string('a', 243);
        var email = $"{localPart}@example.com";
        var request = ValidOrderRequest(email: email);

        var response = await _client.PostAsJsonAsync($"/api/concerts/{concert.Id}/orders", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_EmailMissing_Returns400()
    {
        ClearDatabase();
        var concert = await CreateConcert();
        var request = new { Qty = 1 };

        var response = await _client.PostAsJsonAsync($"/api/concerts/{concert.Id}/orders", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_QtyOverMaxCapacity_Returns400()
    {
        ClearDatabase();
        var concert = await CreateConcert(maxCapacity: 10);
        var request = ValidOrderRequest(qty: 11);

        var response = await _client.PostAsJsonAsync($"/api/concerts/{concert.Id}/orders", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_QtyExactlyAtRemainingCapacity_Returns201()
    {
        ClearDatabase();
        var concert = await CreateConcert(maxCapacity: 10);
        var request = ValidOrderRequest(qty: 10);

        var response = await _client.PostAsJsonAsync($"/api/concerts/{concert.Id}/orders", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_SequentialOrdersExceedingCapacity_SecondOrderReturns400()
    {
        ClearDatabase();
        var concert = await CreateConcert(maxCapacity: 10);

        var first = await _client.PostAsJsonAsync(
            $"/api/concerts/{concert.Id}/orders", ValidOrderRequest(qty: 6));
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await _client.PostAsJsonAsync(
            $"/api/concerts/{concert.Id}/orders", ValidOrderRequest(qty: 6));

        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_ConcertAlreadySoldOut_Returns400()
    {
        ClearDatabase();
        var concert = await CreateConcert(maxCapacity: 5);

        var soldOut = await _client.PostAsJsonAsync(
            $"/api/concerts/{concert.Id}/orders", ValidOrderRequest(qty: 5));
        Assert.Equal(HttpStatusCode.Created, soldOut.StatusCode);

        var response = await _client.PostAsJsonAsync(
            $"/api/concerts/{concert.Id}/orders", ValidOrderRequest(qty: 1));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_QtyBelowMaxCapacity_IncreasesTicketsSold()
    {
        ClearDatabase();
        var concert = await CreateConcert(maxCapacity: 100);

        await _client.PostAsJsonAsync($"/api/concerts/{concert.Id}/orders", ValidOrderRequest(qty: 7));

        var getResponse = await _client.GetAsync($"/api/concerts/{concert.Id}");
        var updatedConcert = await getResponse.Content.ReadFromJsonAsync<ConcertResponse>();

        Assert.NotNull(updatedConcert);
        Assert.Equal(7, updatedConcert.TicketsSold);
    }

    [Fact]
    public async Task CreateOrder_MultipleSequentialOrders_AccumulateTicketsSold()
    {
        ClearDatabase();
        var concert = await CreateConcert(maxCapacity: 100);

        await _client.PostAsJsonAsync($"/api/concerts/{concert.Id}/orders", ValidOrderRequest(qty: 3));
        await _client.PostAsJsonAsync($"/api/concerts/{concert.Id}/orders", ValidOrderRequest(qty: 4));

        var getResponse = await _client.GetAsync($"/api/concerts/{concert.Id}");
        var updatedConcert = await getResponse.Content.ReadFromJsonAsync<ConcertResponse>();

        Assert.NotNull(updatedConcert);
        Assert.Equal(7, updatedConcert.TicketsSold);
    }

    [Fact]
    public async Task CreateOrder_TotalPriceIsQtyTimesUnitPrice()
    {
        ClearDatabase();
        var concert = await CreateConcert(ticketPrice: 19.99m);
        var request = ValidOrderRequest(qty: 3);

        var response = await _client.PostAsJsonAsync($"/api/concerts/{concert.Id}/orders", request);
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();

        Assert.NotNull(order);
        Assert.Equal(59.97m, order.TotalPrice);
    }

    [Fact]
    public async Task CreateOrder_ZeroUnitPriceConcert_Returns201()
    {
        ClearDatabase();
        var concert = await CreateConcert(ticketPrice: 0);
        var request = ValidOrderRequest();

        var response = await _client.PostAsJsonAsync($"/api/concerts/{concert.Id}/orders", request);
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(order);
        Assert.Equal(0m, order.UnitPrice);
        Assert.Equal(0m, order.TotalPrice);
    }

    [Fact]
    public async Task CreateOrder_PositiveUnitPriceConcert_Returns201()
    {
        ClearDatabase();
        var concert = await CreateConcert(ticketPrice: 42.50m);
        var request = ValidOrderRequest();

        var response = await _client.PostAsJsonAsync($"/api/concerts/{concert.Id}/orders", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_UnitPriceIsAssignedFromConcertTicketPrice()
    {
        ClearDatabase();
        var concert = await CreateConcert(ticketPrice: 123.45m);
        var request = ValidOrderRequest();

        var response = await _client.PostAsJsonAsync($"/api/concerts/{concert.Id}/orders", request);
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();

        Assert.NotNull(order);
        Assert.Equal(123.45m, order.UnitPrice);
    }

    [Fact]
    public async Task CreateOrder_ConcertIdIsAssignedCorrectly()
    {
        ClearDatabase();
        var concert = await CreateConcert();
        var request = ValidOrderRequest();

        var response = await _client.PostAsJsonAsync($"/api/concerts/{concert.Id}/orders", request);
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();

        Assert.NotNull(order);
        Assert.Equal(concert.Id, order.ConcertId);
    }

    [Fact]
    public async Task CreateOrder_ResponseHasNonEmptyOrderId()
    {
        ClearDatabase();
        var concert = await CreateConcert();
        var request = ValidOrderRequest();

        var response = await _client.PostAsJsonAsync($"/api/concerts/{concert.Id}/orders", request);
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();

        Assert.NotNull(order);
        Assert.NotEqual(Guid.Empty, order.Id);
    }
    
    [Fact]
    public async Task CreateOrder_TwoRequestsRaceOnSameConcert_SecondRequestReturns409Conflict()
    {
        ClearDatabase();
        var concert = await CreateConcert();
        var request = ValidOrderRequest(qty: 1);

        using var scope1 = _factory.Services.CreateScope();
        using var scope2 = _factory.Services.CreateScope();

        var controller1 = ActivatorUtilities.CreateInstance<ConcertsController>(scope1.ServiceProvider);
        var controller2 = ActivatorUtilities.CreateInstance<ConcertsController>(scope2.ServiceProvider);

        var db2 = scope2.ServiceProvider.GetRequiredService<AppDbContext>();
        _ = db2.Concerts.Single(c => c.Id == concert.Id);

        var result1 = controller1.CreateOrder(concert.Id, request);
        var created1 = Assert.IsType<ObjectResult>(result1);
        Assert.Equal(StatusCodes.Status201Created, created1.StatusCode);

        var result2 = controller2.CreateOrder(concert.Id, request);

        var conflict = Assert.IsType<ConflictObjectResult>(result2);
        Assert.Equal(StatusCodes.Status409Conflict, conflict.StatusCode);
    }
    
    [Fact]
    public async Task CreateOrder_TwoRequestsOnDifferentConcerts_BothSucceed()
    {
        ClearDatabase();
        var concertA = await CreateConcert();
        var concertB = await CreateConcert();
        var request = ValidOrderRequest(qty: 1);

        using var scope1 = _factory.Services.CreateScope();
        using var scope2 = _factory.Services.CreateScope();

        var controller1 = ActivatorUtilities.CreateInstance<ConcertsController>(scope1.ServiceProvider);
        var controller2 = ActivatorUtilities.CreateInstance<ConcertsController>(scope2.ServiceProvider);

        var db2 = scope2.ServiceProvider.GetRequiredService<AppDbContext>();
        _ = db2.Concerts.Single(c => c.Id == concertB.Id);

        var result1 = controller1.CreateOrder(concertA.Id, request);
        var result2 = controller2.CreateOrder(concertB.Id, request);

        Assert.Equal(StatusCodes.Status201Created, ((ObjectResult)result1).StatusCode);
        Assert.Equal(StatusCodes.Status201Created, ((ObjectResult)result2).StatusCode);
    }
}