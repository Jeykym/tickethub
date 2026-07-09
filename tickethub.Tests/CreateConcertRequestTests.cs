using System.ComponentModel.DataAnnotations;
using tickethub.Dtos;

namespace tickethub.Tests;

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
        var request = new CreateConcertRequest(
            "Metallica",
            DateTime.UtcNow.AddDays(10),
            5000,
            675
        );
        
        var results = ValidateModel(request);
        
        Assert.Empty(results);
    }
    
    [Fact]
    public void EmptyTitle_ShouldFailValidation()
    {
        var request = new CreateConcertRequest(
            string.Empty,
            DateTime.UtcNow.AddDays(10),
            5000,
            675
        );

        var results = ValidateModel(request);

        Assert.Single(results);
    }
    
    [Fact]
    public void SingleCharacterTitle_ShouldPassValidation()
    {
        var request = new CreateConcertRequest(
            "a",
            DateTime.UtcNow.AddDays(10),
            5000,
            675
        );

        var results = ValidateModel(request);

        Assert.Empty(results);
    }
    
    [Fact]
    public void TitleAtMaxLength_ShouldPassValidation()
    {
        var request = new CreateConcertRequest(
            new string('a', 50),
            DateTime.UtcNow.AddDays(10),
            5000,
            675
        );

        var results = ValidateModel(request);

        Assert.Empty(results);
    }

    [Fact]
    public void TitleExceedingMaxLength_ShouldFailValidation()
    {
        var request = new CreateConcertRequest(
            new string('a', 51),
            DateTime.UtcNow.AddDays(10),
            5000,
            675
        );

        var result = ValidateModel(request);
        
        Assert.Single(result);
    }
    
    [Fact]
    public void ZeroMaxCapacity_ShouldFailValidation()
    {
        var request = new CreateConcertRequest(
            "Metallica",
            DateTime.UtcNow.AddDays(10),
            0,
            675
        );

        var results = ValidateModel(request);

        Assert.Single(results);
    }
    
    [Fact]
    public void PositiveMaxCapacity_ShouldPassValidation()
    {
        var request = new CreateConcertRequest(
            "Metallica",
            DateTime.UtcNow.AddDays(10),
            1,
            675
        );

        var results = ValidateModel(request);

        Assert.Empty(results);
    }
    
    [Fact]
    public void NegativeMaxCapacity_ShouldFailValidation()
    {
        var request = new CreateConcertRequest(
            "Metallica",
            DateTime.UtcNow.AddDays(10),
            -1,
            675
        );

        var results = ValidateModel(request);

        Assert.Single(results);
    }
    
    [Fact]
    public void ZeroTicketPrice_ShouldPassValidation()
    {
        var request = new CreateConcertRequest(
            "Metallica",
            DateTime.UtcNow.AddDays(10),
            5000,
            0
        );

        var results = ValidateModel(request);

        Assert.Empty(results);
    }
    
    [Fact]
    public void PositiveTicketPrice_ShouldPassValidation()
    {
        var request = new CreateConcertRequest(
            "Metallica",
            DateTime.UtcNow.AddDays(10),
            5000,
            1
        );

        var results = ValidateModel(request);

        Assert.Empty(results);
    }
    
    [Fact]
    public void NegativeTicketPrice_ShouldFailValidation()
    {
        var request = new CreateConcertRequest(
            "Metallica",
            DateTime.UtcNow.AddDays(10),
            5000,
            -1
        );

        var results = ValidateModel(request);

        Assert.Single(results);
    }
}