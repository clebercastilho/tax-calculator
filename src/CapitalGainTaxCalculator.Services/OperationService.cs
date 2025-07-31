using CapitalGainTaxCalculator.Domain.Models;
using CapitalGainTaxCalculator.Domain.Contracts;
using Microsoft.Extensions.Logging;

namespace CapitalGainTaxCalculator.Services;

public class OperationService(ITaxService taxService, ILogger<OperationService> logger) 
    : IOperationService
{
    readonly ITaxService _taxService = taxService;
    readonly ILogger<OperationService> _logger = logger;

    public List<OperationResult> CalculateTransactionsTax(List<Operation> operations)
    {
        var operationsResults = new List<OperationResult>();

        if (operations == null || operations.Count == 0)
        {
            _logger.LogWarning("A filled operation list is required for calc");
            return operationsResults;
        }

        try
            {
                int stockQuantity = 0;
                decimal currentWeightedAverage = 0;
                decimal acumulatedLosses = 0;
                decimal minimalProfitToTax = _taxService.MinimalProfitToTax();

                foreach (var operation in operations)
                {
                    if (operation.OperationType == OperationType.Buy)
                    {
                        var newWeightedAverage = ((stockQuantity * currentWeightedAverage) + (operation.Quantity * operation.UnitCost)) / (stockQuantity + operation.Quantity);

                        currentWeightedAverage = newWeightedAverage;
                        stockQuantity += operation.Quantity;

                        //there's no tax on buy operations
                        operationsResults.Add(
                            new OperationResult { Tax = 0, CurrentWeightedAverage = currentWeightedAverage });

                        continue;
                    }

                    if (operation.OperationType == OperationType.Sell)
                    {
                        decimal taxDue = 0, operationAmount = 0, operationProfit = 0, finalProfit = 0;

                        if (stockQuantity >= operation.Quantity)
                        {
                            operationAmount = operation.Quantity * operation.UnitCost;

                            operationProfit = operationAmount - (operation.Quantity * currentWeightedAverage);

                            //tive lucro numa operação maior que 20k?
                            if (operationAmount > minimalProfitToTax && operationProfit > 0)
                            {
                                //tenho prejuizos à deduzir?
                                if (acumulatedLosses < 0)
                                {
                                    acumulatedLosses += operationProfit;

                                    if (acumulatedLosses > 0)
                                    {
                                        finalProfit = acumulatedLosses;
                                        acumulatedLosses = 0;
                                    }
                                }
                                else
                                    finalProfit = operationProfit;
                            }

                            //ou tive prejuizo?
                            if (operationProfit < 0)
                            {
                                acumulatedLosses += operationProfit;
                            }

                            if (finalProfit > 0 && operationAmount > minimalProfitToTax)
                                taxDue = _taxService.CalculateTax(finalProfit);

                            stockQuantity -= operation.Quantity;
                        }

                        operationsResults.Add(
                            new OperationResult { Tax = taxDue, CurrentProfit = operationProfit, CurrentWeightedAverage = currentWeightedAverage });

                        continue;
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message);
            }

        return operationsResults;
    }
}
