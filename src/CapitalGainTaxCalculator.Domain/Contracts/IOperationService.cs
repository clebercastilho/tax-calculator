using CapitalGainTaxCalculator.Domain.Models;

namespace CapitalGainTaxCalculator.Domain.Contracts;

public interface IOperationService
{
    List<OperationResult> CalculateTransactionsTax(List<Operation> operations);
}