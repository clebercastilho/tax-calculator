namespace CapitalGainTaxCalculator.Domain.Contracts;

public interface ITaxService
{
    int MinimalProfitToTax();
    decimal CalculateTax(decimal profit);
}