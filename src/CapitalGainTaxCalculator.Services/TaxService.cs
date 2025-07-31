using CapitalGainTaxCalculator.Domain.Contracts;

namespace CapitalGainTaxCalculator.Services;

public class TaxService : ITaxService
{
    readonly int _minimalProfitToTax = 20000;
    readonly decimal _taxPercentual = 0.2M;

    public int MinimalProfitToTax() => _minimalProfitToTax;

    public decimal CalculateTax(decimal profit) => (profit * _taxPercentual);
}