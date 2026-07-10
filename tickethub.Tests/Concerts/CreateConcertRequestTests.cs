using System.ComponentModel.DataAnnotations;
using tickethub.Dtos;

namespace tickethub.Tests.Concerts;

public class CreateConcertRequestTests
{

    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }
    
    [Fact]
    public void ValidRequest_ShouldPassValidation()
    {
        var request = new CreateConcertRequest
        {
            Title = "Metallica",
            Start = DateTime.UtcNow.AddDays(10),
            MaxCapacity = 5000,
            TicketPrice = 675
        };
        
        var results = ValidateModel(request);
        
        Assert.Empty(results);
    }
    
    [Fact]
    public void EmptyTitle_ShouldFailValidation()
    {
        var request = new CreateConcertRequest
        {
            Title = string.Empty,
            Start = DateTime.UtcNow.AddDays(10),
            MaxCapacity = 5000,
            TicketPrice = 675
        };

        var results = ValidateModel(request);

        Assert.Single(results);
    }
    
    [Fact]
    public void SingleCharacterTitle_ShouldPassValidation()
    {
        var request = new CreateConcertRequest
        {
            Title = "a",
            Start = DateTime.UtcNow.AddDays(10),
            MaxCapacity = 5000,
            TicketPrice = 675
        };

        var results = ValidateModel(request);

        Assert.Empty(results);
    }
    
    [Fact]
    public void TitleAtMaxLength_ShouldPassValidation()
    {
        var request = new CreateConcertRequest
        {
            Title = new string('a', 50),
            Start = DateTime.UtcNow.AddDays(10),
            MaxCapacity = 5000,
            TicketPrice = 675
        };

        var results = ValidateModel(request);

        Assert.Empty(results);
    }

    [Fact]
    public void TitleExceedingMaxLength_ShouldFailValidation()
    {
        var request = new CreateConcertRequest
        {
            Title = new string('a', 51),
            Start = DateTime.UtcNow.AddDays(10),
            MaxCapacity = 5000,
            TicketPrice = 675
        };

        var result = ValidateModel(request);
        
        Assert.Single(result);
    }
    
    [Fact]
    public void ZeroMaxCapacity_ShouldFailValidation()
    {
        var request = new CreateConcertRequest
        {
            Title = "Metallica",
            Start = DateTime.UtcNow.AddDays(10),
            MaxCapacity = 0,
            TicketPrice = 675
        };

        var results = ValidateModel(request);

        Assert.Single(results);
    }
    
    [Fact]
    public void PositiveMaxCapacity_ShouldPassValidation()
    {
        var request = new CreateConcertRequest
        {
            Title = "Metallica",
            Start = DateTime.UtcNow.AddDays(10),
            MaxCapacity = 1,
            TicketPrice = 675
        };

        var results = ValidateModel(request);

        Assert.Empty(results);
    }
    
    [Fact]
    public void NegativeMaxCapacity_ShouldFailValidation()
    {
        var request = new CreateConcertRequest
        {
            Title = "Metallica",
            Start = DateTime.UtcNow.AddDays(10),
            MaxCapacity = -1,
            TicketPrice = 675
        };

        var results = ValidateModel(request);

        Assert.Single(results);
    }
    
    [Fact]
    public void ZeroTicketPrice_ShouldPassValidation()
    {
        var request = new CreateConcertRequest
        {
            Title = "Metallica",
            Start = DateTime.UtcNow.AddDays(10),
            MaxCapacity = 5000,
            TicketPrice = 0
        };

        var results = ValidateModel(request);

        Assert.Empty(results);
    }
    
    [Fact]
    public void PositiveTicketPrice_ShouldPassValidation()
    {
        var request = new CreateConcertRequest
        {
            Title = "Metallica",
            Start = DateTime.UtcNow.AddDays(10),
            MaxCapacity = 5000,
            TicketPrice = 1
        };

        var results = ValidateModel(request);

        Assert.Empty(results);
    }
    
    [Fact]
    public void NegativeTicketPrice_ShouldFailValidation()
    {
        var request = new CreateConcertRequest
        {
            Title = "Metallica",
            Start = DateTime.UtcNow.AddDays(10),
            MaxCapacity = 5000,
            TicketPrice = -1
        };

        var results = ValidateModel(request);

        Assert.Single(results);
    }
}