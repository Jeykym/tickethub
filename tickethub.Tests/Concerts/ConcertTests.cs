using System.ComponentModel.DataAnnotations;
using tickethub.Models;

namespace tickethub.Tests.Concerts;

public class ConcertTests
{

    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }

    
    private static Concert CreateValidConcert(
        string title = "Metallica",
        int maxCapacity = 5000,
        decimal ticketPrice = 675,
        int ticketsSold = 0)
    {
        return new Concert
        {
            Title = title,
            Start = DateTime.UtcNow.AddDays(10),
            MaxCapacity = maxCapacity,
            TicketPrice = ticketPrice,
            TicketsSold = ticketsSold
        };
    }

    [Fact]
    public void ValidConcert_ShouldPassValidation()
    {
        var concert = CreateValidConcert();
        var results = ValidateModel(concert);
        Assert.Empty(results);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    public void ValidTitleLengths_ShouldPassValidation(int length)
    {
        var concert = CreateValidConcert(title: new string('a', length));
        var results = ValidateModel(concert);
        Assert.Empty(results);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public void InvalidTitleLengths_ShouldFailValidation(int length)
    {
        var concert = CreateValidConcert(title: new string('a', length));
        var results = ValidateModel(concert);
        Assert.Single(results);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5000)]
    public void ValidMaxCapacity_ShouldPassValidation(int capacity)
    {
        var concert = CreateValidConcert(maxCapacity: capacity);
        var results = ValidateModel(concert);
        Assert.Empty(results);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void InvalidMaxCapacity_ShouldFailValidation(int capacity)
    {
        var concert = CreateValidConcert(maxCapacity: capacity);
        var results = ValidateModel(concert);
        Assert.Single(results);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(675)]
    public void ValidTicketPrice_ShouldPassValidation(decimal price)
    {
        var concert = CreateValidConcert(ticketPrice: price);
        var results = ValidateModel(concert);
        Assert.Empty(results);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void InvalidTicketPrice_ShouldFailValidation(decimal price)
    {
        var concert = CreateValidConcert(ticketPrice: price);
        var results = ValidateModel(concert);
        Assert.Single(results);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void ValidTicketsSold_ShouldPassValidation(int sold)
    {
        var concert = CreateValidConcert(ticketsSold: sold);
        var results = ValidateModel(concert);
        Assert.Empty(results);
    }

    [Theory]
    [InlineData(-1)]
    public void InvalidTicketsSold_ShouldFailValidation(int sold)
    {
        var concert = CreateValidConcert(ticketsSold: sold);
        var results = ValidateModel(concert);
        Assert.Single(results);
    }
}