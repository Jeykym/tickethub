using System.ComponentModel.DataAnnotations;
using tickethub.Dtos.Order;

namespace tickethub.Tests.Orders;

public class CreateOrderRequestTests
{
    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }

    private static CreateOrderRequest CreateValidRequest(
        string customerEmail = "customer@example.com",
        int qty = 1)
    {
        return new CreateOrderRequest
        {
            CustomerEmail = customerEmail,
            Qty = qty,
        };
    }

    [Fact]
    public void ValidRequest_ShouldPassValidation()
    {
        var request = CreateValidRequest();
        var results = ValidateModel(request);
        Assert.Empty(results);
    }

    [Fact]
    public void EmptyEmail_ShouldFailValidation()
    {
        var request = CreateValidRequest(customerEmail: string.Empty);
        var results = ValidateModel(request);
        Assert.NotEmpty(results);
    }

    [Fact]
    public void EmailMissingAtSymbol_ShouldFailValidation()
    {
        var request = CreateValidRequest(customerEmail: "customerexample.com");
        var results = ValidateModel(request);
        Assert.Single(results);
    }

    [Fact]
    public void EmailWithEmptyLocalPart_ShouldFailValidation()
    {
        var request = CreateValidRequest(customerEmail: "@example.com");
        var results = ValidateModel(request);
        Assert.Single(results);
    }

    [Fact]
    public void EmailWithEmptyDomainPart_ShouldFailValidation()
    {
        var request = CreateValidRequest(customerEmail: "customer@");
        var results = ValidateModel(request);
        Assert.Single(results);
    }

    [Fact]
    public void EmailWith254Characters_ShouldPassValidation()
    {
        const string domainPart = "@a.com";
        var localPart = new string('a', 254 - domainPart.Length); 
        var email = $"{localPart}{domainPart}";
        
        var request = CreateValidRequest(customerEmail: email);
        var results = ValidateModel(request);
        
        Assert.Empty(results);
    }

    [Fact]
    public void EmailWith255Characters_ShouldFailValidation()
    {
        const string domainPart = "@a.com";
        var localPart = new string('a', 255 - domainPart.Length); 
        var email = $"{localPart}{domainPart}";
        
        var request = CreateValidRequest(customerEmail: email);
        var results = ValidateModel(request);
        
        Assert.Single(results);
    }

    [Fact]
    public void ZeroQty_ShouldFailValidation()
    {
        var request = CreateValidRequest(qty: 0);
        var results = ValidateModel(request);
        Assert.Single(results);
    }

    [Fact]
    public void NegativeQty_ShouldFailValidation()
    {
        var request = CreateValidRequest(qty: -1);
        var results = ValidateModel(request);
        Assert.Single(results);
    }

    [Fact]
    public void PositiveQty_ShouldPassValidation()
    {
        var request = CreateValidRequest(qty: 1);
        var results = ValidateModel(request);
        Assert.Empty(results);
    }
}