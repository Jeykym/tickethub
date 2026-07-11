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

namespace tickethub.Tests.Concerts;

public class ConcertsControllerTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly SqliteConnection _connection;

    public ConcertsControllerTests(WebApplicationFactory<Program> factory)
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

    private static CreateConcertRequest ValidRequest(
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

    private void ClearDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Concerts.RemoveRange(db.Concerts);
        db.SaveChanges();
    }

    [Fact]
    public async Task CreateConcert_ValidRequest_Returns201()
    {
        var request = ValidRequest();
        
        var response = await _client.PostAsJsonAsync("/api/concerts", request);
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var concert = await response.Content.ReadFromJsonAsync<ConcertResponse>();
        
        Assert.NotNull(concert);
        Assert.Equal(concert.Title, request.Title);
    }

    [Fact]
    public async Task CreateConcert_MissingTitle_Returns400()
    {
        var request = new { Start = DateTime.Now, MaxCapacity = 5000, TicketPrice = 675m };
        var response = await _client.PostAsJsonAsync("/api/concerts", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateConcert_MissingStart_Returns400()
    {
        var request = new { Title = "Metallica", MaxCapacity = 5000, TicketPrice = 675m };
        var response = await _client.PostAsJsonAsync("/api/concerts", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateConcert_MissingMaxCapacity_Returns400()
    {
        var request = new { Title = "Metallica", Start = DateTime.Now, TicketPrice = 675m };
        var response = await _client.PostAsJsonAsync("/api/concerts", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateConcert_MissingTicketPrice_Returns400()
    {
        var request = new { Title = "Metallica", Start = DateTime.Now, MaxCapacity = 5000 };
        var response = await _client.PostAsJsonAsync("/api/concerts", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(25)]
    [InlineData(1)]
    [InlineData(50)]
    public async Task CreateConcert_ValidTitleLengths_Returns201(int length)
    {
        var request = ValidRequest(title: new string('a', length));
        var response = await _client.PostAsJsonAsync("/api/concerts", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public async Task CreateConcert_InvalidTitleLengths_Returns400(int length)
    {
        var request = ValidRequest(title: new string('a', length));
        var response = await _client.PostAsJsonAsync("/api/concerts", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5000)]
    public async Task CreateConcert_ValidMaxCapacity_Returns201(int capacity)
    {
        var request = ValidRequest(maxCapacity: capacity);
        var response = await _client.PostAsJsonAsync("/api/concerts", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateConcert_InvalidMaxCapacity_Returns400(int capacity)
    {
        var request = ValidRequest(maxCapacity: capacity);
        var response = await _client.PostAsJsonAsync("/api/concerts", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100.50)]
    public async Task CreateConcert_ValidTicketPrice_Returns201(decimal price)
    {
        var request = ValidRequest(ticketPrice: price);
        var response = await _client.PostAsJsonAsync("/api/concerts", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public async Task CreateConcert_InvalidTicketPrice_Returns400(decimal price)
    {
        var request = ValidRequest(ticketPrice: price);
        var response = await _client.PostAsJsonAsync("/api/concerts", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task GetAllConcerts_NoConcertsStored_Returns200WithEmptyList()
    {
        ClearDatabase();

        var response = await _client.GetAsync("/api/concerts");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var concerts = await response.Content.ReadFromJsonAsync<ConcertResponse[]>();
        Assert.NotNull(concerts);
        Assert.Empty(concerts);
    }

    [Fact]
    public async Task GetAllConcerts_SingleConcertStored_Returns200WithOneConcert()
    {
        ClearDatabase();
        var request = ValidRequest(title: "Solo Concert");
        await _client.PostAsJsonAsync("/api/concerts", request);

        var response = await _client.GetAsync("/api/concerts");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var concerts = await response.Content.ReadFromJsonAsync<ConcertResponse[]>();
        Assert.NotNull(concerts);
        var concert = Assert.Single(concerts);
        Assert.Equal("Solo Concert", concert.Title);
    }

    [Fact]
    public async Task GetAllConcerts_MultipleConcertsStored_Returns200WithAllConcerts()
    {
        ClearDatabase();
        var request1 = ValidRequest(title: "Concert A");
        var request2 = ValidRequest(title: "Concert B");
        await _client.PostAsJsonAsync("/api/concerts", request1);
        await _client.PostAsJsonAsync("/api/concerts", request2);

        var response = await _client.GetAsync("/api/concerts");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var concerts = await response.Content.ReadFromJsonAsync<ConcertResponse[]>();
        Assert.NotNull(concerts);
        Assert.Equal(2, concerts.Length);
        Assert.Contains(concerts, c => c.Title == "Concert A");
        Assert.Contains(concerts, c => c.Title == "Concert B");
    }
    
    [Fact]
    public async Task GetConcertById_ValidId_ReturnsConcert()
    {
        ClearDatabase();
        var request = ValidRequest(title: "Solo Concert");
        var createResponse = await _client.PostAsJsonAsync("/api/concerts", request);
        var created = await createResponse.Content.ReadFromJsonAsync<ConcertResponse>();
        Assert.NotNull(created);

        var response = await _client.GetAsync($"/api/concerts/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var concert = await response.Content.ReadFromJsonAsync<ConcertResponse>();
        Assert.NotNull(concert);
        Assert.Equal(created.Id, concert.Id);
        Assert.Equal("Solo Concert", concert.Title);
    }

    [Fact]
    public async Task GetConcertById_InvalidId_Returns404()
    {
        ClearDatabase();

        var response = await _client.GetAsync("/api/concerts/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task DeleteConcert_ValidId_Returns204NoContentAndRemovesConcert()
    {
        ClearDatabase();
        var request = ValidRequest(title: "Concert to Delete");
        var createResponse = await _client.PostAsJsonAsync("/api/concerts", request);
        var created = await createResponse.Content.ReadFromJsonAsync<ConcertResponse>();
        Assert.NotNull(created);

        var deleteResponse = await _client.DeleteAsync($"/api/concerts/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/concerts/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteConcert_InvalidId_Returns404NotFound()
    {
        ClearDatabase();

        var response = await _client.DeleteAsync($"/api/concerts/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}