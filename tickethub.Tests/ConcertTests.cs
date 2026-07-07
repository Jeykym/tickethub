using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using tickethub.Models;
using Xunit;

namespace tickethub.Tests;

public class ConcertTests
{
    private IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }
    
    [Fact]
    public void ValidConcert_ShouldPassValidation()
    {
        var concert = new Concert
        {
            Title = "Metallica",
            Start = DateTime.UtcNow.AddDays(10),
            MaxCapacity = 5000,
            TicketPrice = 675
        };
        
        var results = ValidateModel(concert);
        
        Assert.Empty(results);
    }
    
    [Fact]
    public void EmptyTitle_ShouldFailValidation()
    {
        var concert = new Concert
        {
            Title = string.Empty,
            Start = DateTime.UtcNow.AddDays(10),
            MaxCapacity = 5000,
            TicketPrice = 675
        };

        var results = ValidateModel(concert);

        Assert.Single(results);
    }
    
    [Fact]
    public void SingleCharacterTitle_ShouldPassValidation()
    {
        var concert = new Concert
        {
            Title = "a",
            Start = DateTime.UtcNow.AddDays(10),
            MaxCapacity = 5000,
            TicketPrice = 675
        };

        var results = ValidateModel(concert);

        Assert.Empty(results);
    }
    
    
    [Fact]
    public void TitleAtMaxLength_ShouldPassValidation()
    {
        var concert = new Concert
        {
            Title = new string('a', 50),   // exactly 50 — boundary case
            Start = DateTime.UtcNow.AddDays(10),
            MaxCapacity = 5000,
            TicketPrice = 675
        };

        var results = ValidateModel(concert);

        Assert.Empty(results);
    }

    [Fact]
    public void TitleExceedingMaxLength_ShouldFailValidation()
    {
        var concert = new Concert
        {
            Title = new string('a', 51),
            Start = DateTime.UtcNow.AddDays(10),
            MaxCapacity = 5000,
            TicketPrice = 675
        };

        var result = ValidateModel(concert);
        
        Assert.Single(result);
    }
    
    
    [Fact]
    public void ZeroMaxCapacity_ShouldFailValidation()
    {
        var concert = new Concert
        {
            Title = "Metallica",
            Start = DateTime.UtcNow.AddDays(10),
            MaxCapacity = 0,
            TicketPrice = 675
        };

        var results = ValidateModel(concert);

        Assert.Single(results);
    }
    
    [Fact]
    public void PositiveMaxCapacity_ShouldPassValidation()
    {
        var concert = new Concert
        {
            Title = "Metallica",
            Start = DateTime.UtcNow.AddDays(10),
            MaxCapacity = 1,
            TicketPrice = 675
        };

        var results = ValidateModel(concert);

        Assert.Empty(results);
    }
    
    [Fact]
    public void NegativeMaxCapacity_ShouldFailValidation()
    {
        var concert = new Concert
        {
            Title = "Metallica",
            Start = DateTime.UtcNow.AddDays(10),
            MaxCapacity = -1,
            TicketPrice = 675
        };

        var results = ValidateModel(concert);

        Assert.Single(results);
    }
    
    [Fact]
    public void ZeroTicketPrice_ShouldPassValidation()
    {
        var concert = new Concert
        {
            Title = "Metallica",
            Start = DateTime.UtcNow.AddDays(10),
            MaxCapacity = 5000,
            TicketPrice = 0
        };

        var results = ValidateModel(concert);

        Assert.Empty(results);
    }
    
    [Fact]
    public void PositiveTicketPrice_ShouldPassValidation()
    {
        var concert = new Concert
        {
            Title = "Metallica",
            Start = DateTime.UtcNow.AddDays(10),
            MaxCapacity = 5000,
            TicketPrice = 1
        };

        var results = ValidateModel(concert);

        Assert.Empty(results);
    }
    
    [Fact]
    public void NegativeTicketPrice_ShouldFailValidation()
    {
        var concert = new Concert
        {
            Title = "Metallica",
            Start = DateTime.UtcNow.AddDays(10),
            MaxCapacity = 5000,
            TicketPrice = -1
        };

        var results = ValidateModel(concert);

        Assert.Single(results);
    }
    
    [Fact]
    public void PositiveTicketsSold_ShouldPassValidation()
    {
        var concert = new Concert
        {
            Title = "Metallica",
            Start = DateTime.UtcNow.AddDays(10),
            MaxCapacity = 5000,
            TicketPrice = 675,
            TicketsSold = 1
        };

        var results = ValidateModel(concert);

        Assert.Empty(results);
    }
    
    [Fact]
    public void ZeroTicketsSold_ShouldPassValidation()
    {
        var concert = new Concert
        {
            Title = "Metallica",
            Start = DateTime.UtcNow.AddDays(10),
            MaxCapacity = 5000,
            TicketPrice = 675,
            TicketsSold = 0
        };

        var results = ValidateModel(concert);

        Assert.Empty(results);
    }
    
    [Fact]
    public void NegativeTicketsSold_ShouldFailValidation()
    {
        var concert = new Concert
        {
            Title = "Metallica",
            Start = DateTime.UtcNow.AddDays(10),
            MaxCapacity = 5000,
            TicketPrice = 675,
            TicketsSold = -1
        };

        var results = ValidateModel(concert);

        Assert.Single(results);
    }
}