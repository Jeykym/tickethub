using System.Data.Common;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using tickethub.Dtos;
using tickethub.Dtos.Concert;
using tickethub.Dtos.Order;

namespace tickethub.Tests.Orders;

public class OrdersControllerTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly SqliteConnection _connection;

    public OrdersControllerTests(WebApplicationFactory<Program> factory)
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
        string customerEmail = "customer@example.com",
        int qty = 2)
    {
        return new CreateOrderRequest
        {
            CustomerEmail = customerEmail,
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

    private async Task<ConcertResponse> CreateConcertAsync(
        string title = "Metallica",
        int maxCapacity = 5000,
        decimal ticketPrice = 675)
    {
        var request = ValidConcertRequest(title: title, maxCapacity: maxCapacity, ticketPrice: ticketPrice);
        var response = await _client.PostAsJsonAsync("/api/concerts", request);
        var concert = await response.Content.ReadFromJsonAsync<ConcertResponse>();
        Assert.NotNull(concert);
        return concert;
    }

    private async Task<OrderResponse> CreateOrderAsync(
        int concertId,
        string customerEmail = "customer@example.com",
        int qty = 2)
    {
        var request = ValidOrderRequest(customerEmail: customerEmail, qty: qty);
        var response = await _client.PostAsJsonAsync($"/api/concerts/{concertId}/orders", request);
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.NotNull(order);
        return order;
    }

    [Fact]
    public async Task GetOrders_NoOrdersStored_Returns200WithEmptyList()
    {
        ClearDatabase();

        var response = await _client.GetAsync("/api/orders");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var orders = await response.Content.ReadFromJsonAsync<OrderResponse[]>();
        Assert.NotNull(orders);
        Assert.Empty(orders);
    }

    [Fact]
    public async Task GetOrders_SingleOrderStored_Returns200WithOneOrder()
    {
        ClearDatabase();
        var concert = await CreateConcertAsync(title: "Solo Concert");
        await CreateOrderAsync(concert.Id, customerEmail: "solo@example.com", qty: 3);

        var response = await _client.GetAsync("/api/orders");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var orders = await response.Content.ReadFromJsonAsync<OrderResponse[]>();
        Assert.NotNull(orders);
        var order = Assert.Single(orders);
        Assert.Equal("solo@example.com", order.Email);
        Assert.Equal(3, order.Qty);
        Assert.Equal(concert.Id, order.ConcertId);
    }

    [Fact]
    public async Task GetOrders_MultipleOrdersStored_Returns200WithAllOrders()
    {
        ClearDatabase();
        var concertA = await CreateConcertAsync(title: "Concert A");
        var concertB = await CreateConcertAsync(title: "Concert B");
        await CreateOrderAsync(concertA.Id, customerEmail: "customerA@example.com", qty: 1);
        await CreateOrderAsync(concertB.Id, customerEmail: "customerB@example.com", qty: 4);

        var response = await _client.GetAsync("/api/orders");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var orders = await response.Content.ReadFromJsonAsync<OrderResponse[]>();
        Assert.NotNull(orders);
        Assert.Equal(2, orders.Length);
        Assert.Contains(orders, o => o.Email == "customerA@example.com" && o.Qty == 1);
        Assert.Contains(orders, o => o.Email == "customerB@example.com" && o.Qty == 4);
    }
    
    [Fact]
    public async Task GetOrderById_ValidId_ReturnsOrder()
    {
        ClearDatabase();
        var concert = await CreateConcertAsync(title: "Solo Concert");
        var created = await CreateOrderAsync(concert.Id, customerEmail: "solo@example.com", qty: 3);

        var response = await _client.GetAsync($"/api/orders/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.NotNull(order);
        Assert.Equal(created.Id, order.Id);
        Assert.Equal("solo@example.com", order.Email);
        Assert.Equal(3, order.Qty);
        Assert.Equal(concert.Id, order.ConcertId);
    }

    [Fact]
    public async Task GetOrderById_InvalidId_Returns404()
    {
        ClearDatabase();

        var response = await _client.GetAsync($"/api/orders/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task DeleteOrder_ValidId_Returns204NoContentAndRemovesOrder()
    {
        ClearDatabase();
        var concert = await CreateConcertAsync(title: "Concert to Order From");
        var created = await CreateOrderAsync(concert.Id, customerEmail: "delete-me@example.com", qty: 2);

        var deleteResponse = await _client.DeleteAsync($"/api/orders/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/orders/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteOrder_InvalidId_Returns404NotFound()
    {
        ClearDatabase();

        var response = await _client.DeleteAsync($"/api/orders/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}