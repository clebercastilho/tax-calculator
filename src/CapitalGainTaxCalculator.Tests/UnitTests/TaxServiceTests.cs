using CapitalGainTaxCalculator.Services;

namespace CapitalGainTaxCalculator.Tests;

public class TaxServiceTests
{
    [Fact]
    public void When_MinimalProfitTaxCall_Expected_MinimalValueReturns()
    {
        var taxService = new TaxService();
        var results = taxService.MinimalProfitToTax();

        Assert.Equal(20000, results);
    }

    [Fact]
    public void When_CalcTaxOverValidProfitProvided_Expected_ReturnsCalculatedTax()
    {
        decimal expectedValue = 12345 * 0.2M;

        var taxService = new TaxService();
        var results = taxService.CalculateTax(12345);

        Assert.Equal(expectedValue, results);
    }

    [Fact]
    public void When_InvalidProfitIsProvided_Expected_ReturnsZeroTax()
    {
        var taxService = new TaxService();
        var results = taxService.CalculateTax(0);

        Assert.Equal(0, results);
    }
}