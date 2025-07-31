using CapitalGainTaxCalculator.Domain.Models;

namespace CapitalGainTaxCalculator.Tests;

public class OperationModelTests
{
    [Theory]
    [InlineData("Buy")]
    [InlineData("buy")]
    public void When_ValidOperationType_Expected_SucessfullyConvertion(string type)
    {
        var result = Operation.Convert(type);

        Assert.Equal(OperationType.Buy, result);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData("  ")]
    public void When_InvalidOperationType_Expected_DefaultConvertion(string type)
    {
        var result = Operation.Convert(type);

        Assert.Equal(OperationType.None, result);
    }

    [Fact]
    public void When_ValidModelProvided_Expected_SucessfullyConvertToOperationType()
    {
        var operation = new Operation { OperationDesc = "Buy", UnitCost = 20, Quantity = 100 };

        Assert.Equal(OperationType.Buy, operation.OperationType);
    }
}