using CapitalGainTaxCalculator.Domain.Models;
using CapitalGainTaxCalculator.Domain.Contracts;
using CapitalGainTaxCalculator.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CapitalGainTaxCalculator.Tests;

public class OperationServiceTests
{
    readonly ITaxService taxService = new TaxService();
    readonly MockLogger<OperationService> logger = Substitute.For<MockLogger<OperationService>>();

    [Fact]
    public void When_OperationListIsEmpty_Expected_EmptyResults()
    {
        var operations = new List<Operation>();

        var operationService = new OperationService(taxService, logger);

        var results = operationService.CalculateTransactionsTax(operations);

        Assert.Empty(results);
        logger.Received().Log(LogLevel.Warning, Arg.Any<string>());
    }

    [Fact] //cenario #1
    public void When_ProfitsBelow20k_Expected_NoTaxesResults()
    {
        var operations = new List<Operation>
        {
            new() { OperationDesc = "Buy", Quantity = 100, UnitCost = 10 },
            new() { OperationDesc = "Sell", Quantity = 50, UnitCost = 15 },
            new() { OperationDesc = "Sell", Quantity = 50, UnitCost = 15 }
        };

        decimal expectedTax = 0;

        var operationService = new OperationService(taxService, logger);

        var results = operationService.CalculateTransactionsTax(operations);

        Assert.NotEmpty(results);
        Assert.All(results, r => Assert.Equal(expectedTax, r.Tax));
    }

    [Fact] //cenario #2
    public void When_OperationAbove20kNoLosses_Expected_TaxFirstSellOperation()
    {
        var operations = new List<Operation>
        {
            new() { OperationDesc = "Buy", Quantity = 10000, UnitCost = 10 },
            new() { OperationDesc = "Sell", Quantity = 5000, UnitCost = 20 },
            new() { OperationDesc = "Sell", Quantity = 5000, UnitCost = 5 }
        };

        decimal expectedTax = 10000;
        byte expectedResultIndex = 1;

        var operationService = new OperationService(taxService, logger);

        var results = operationService.CalculateTransactionsTax(operations);

        Assert.NotEmpty(results);
        Assert.Equal(operations.Count, results.Count);
        Assert.Equal(expectedTax, results[expectedResultIndex].Tax);
    }

    [Fact] //cenario #3
    public void When_OperationWithLosses_Expected_CalculateTaxOverProfitMinusLosses()
    {
        var operations = new List<Operation>
        {
            new() { OperationDesc = "Buy", Quantity = 10000, UnitCost = 10 },
            new() { OperationDesc = "Sell", Quantity = 5000, UnitCost = 5 },
            new() { OperationDesc = "Sell", Quantity = 3000, UnitCost = 20 }
        };

        decimal expectedTaxDue = 1000;
        byte expectedTaxIndex = 2;

        decimal expectedLosses = -25000;
        byte expectedLossesIndex = 1;

        var operationService = new OperationService(taxService, logger);

        var results = operationService.CalculateTransactionsTax(operations);

        Assert.NotEmpty(results);
        Assert.Equal(operations.Count, results.Count);

        Assert.Equal(expectedLosses, results[expectedLossesIndex].CurrentProfit);

        Assert.Equal(expectedTaxDue, results[expectedTaxIndex].Tax);
    }

    [Fact] //cenario #4
    public void When_MoreThanOneBuyOperationAndSaleWithoutProfitOrLosses_Expected_NoTaxes()
    {
        var operations = new List<Operation>
        {
            new() { OperationDesc = "Buy", Quantity = 10000, UnitCost = 10 },
            new() { OperationDesc = "Buy", Quantity = 5000, UnitCost = 25 },
            new() { OperationDesc = "Sell", Quantity = 10000, UnitCost = 15 }
        };

        decimal expectedTaxDue = 0;
        decimal expectedProfit = 0;
        int expectedOperationIndex = 2;

        var operationService = new OperationService(taxService, logger);

        var results = operationService.CalculateTransactionsTax(operations);

        Assert.NotEmpty(results);
        Assert.Equal(operations.Count, results.Count);

        Assert.Equal(expectedTaxDue, results[expectedOperationIndex].Tax);
        Assert.Equal(expectedProfit, results[expectedOperationIndex].CurrentProfit);
    }

    [Fact] //cenario #5
    public void When_MultipleSalesperationsWithProfit_Expected_TaxesOverLastSale()
    {
        var operations = new List<Operation>
        {
            new() { OperationDesc = "Buy", Quantity = 10000, UnitCost = 10 },
            new() { OperationDesc = "Buy", Quantity = 5000, UnitCost = 25 },
            new() { OperationDesc = "Sell", Quantity = 10000, UnitCost = 15 },
            new() { OperationDesc = "Sell", Quantity = 5000, UnitCost = 25 }
        };

        decimal expectedTaxDue = 10000;
        decimal expectedProfit = 50000;
        int expectedOperationIndex = 3;

        var operationService = new OperationService(taxService, logger);

        var results = operationService.CalculateTransactionsTax(operations);

        Assert.NotEmpty(results);
        Assert.Equal(operations.Count, results.Count);

        Assert.Equal(expectedTaxDue, results[expectedOperationIndex].Tax);
        Assert.Equal(expectedProfit, results[expectedOperationIndex].CurrentProfit);
    }

    [Fact] //cenario #6
    public void When_MultipleSalesWithLossesAndProfits_Expected_ReducingLossesAndTaxOverLastSale()
    {
        var operations = new List<Operation>
        {
            new() { OperationDesc = "Buy", Quantity = 10000, UnitCost = 10 },
            new() { OperationDesc = "Sell", Quantity = 5000, UnitCost = 2 },
            new() { OperationDesc = "Sell", Quantity = 2000, UnitCost = 20 },
            new() { OperationDesc = "Sell", Quantity = 2000, UnitCost = 20 },
            new() { OperationDesc = "Sell", Quantity = 1000, UnitCost = 25 }
        };

        decimal expectedTaxDue = 3000;
        byte expectedOperationIndex = 4;

        var operationService = new OperationService(taxService, logger);

        var results = operationService.CalculateTransactionsTax(operations);

        Assert.NotEmpty(results);
        Assert.Equal(operations.Count, results.Count);

        Assert.Equal(expectedTaxDue, results[expectedOperationIndex].Tax);
    }

    [Fact] //cenario #7
    public void When_MultiplePurchasesAndSales_Expected_ReducingLossesRecalculateAverageAndApplyTaxes()
    {
        var operations = new List<Operation>
        {
            new() { OperationDesc = "Buy", Quantity = 10000, UnitCost = 10 },
            new() { OperationDesc = "Sell", Quantity = 5000, UnitCost = 2 },
            new() { OperationDesc = "Sell", Quantity = 2000, UnitCost = 20 },
            new() { OperationDesc = "Sell", Quantity = 2000, UnitCost = 20 },
            new() { OperationDesc = "Sell", Quantity = 1000, UnitCost = 25 },
            new() { OperationDesc = "Buy", Quantity = 10000, UnitCost = 20 },
            new() { OperationDesc = "Sell", Quantity = 5000, UnitCost = 15 },
            new() { OperationDesc = "Sell", Quantity = 4350, UnitCost = 30 },
            new() { OperationDesc = "Sell", Quantity = 650, UnitCost = 30 }
        };

        decimal expectedTaxDueFirstSale = 3000;
        decimal expectedTaxDueAnotherSale = 3700;
        decimal expectedTaxLastSale = 0;

        var operationService = new OperationService(taxService, logger);

        var results = operationService.CalculateTransactionsTax(operations);

        Assert.NotEmpty(results);
        Assert.Equal(operations.Count, results.Count);

        Assert.Equal(expectedTaxDueFirstSale, results[4].Tax);
        Assert.Equal(expectedTaxDueAnotherSale, results[7].Tax);
        Assert.Equal(expectedTaxLastSale, results[8].Tax);
    }

    [Fact] //cenario #8
    public void When_NoLossesWithBigProfits_Expected_TaxesOverEachProfit()
    {
        var operations = new List<Operation>
        {
            new() { OperationDesc = "Buy", Quantity = 10000, UnitCost = 10 },
            new() { OperationDesc = "Sell", Quantity = 10000, UnitCost = 50 },
            new() { OperationDesc = "Buy", Quantity = 10000, UnitCost = 20 },
            new() { OperationDesc = "Sell", Quantity = 10000, UnitCost = 50 }
        };

        decimal expectedTaxDueFirstSale = 80000;
        decimal expectedTaxDueSecondSale = 60000;

        var operationService = new OperationService(taxService, logger);

        var results = operationService.CalculateTransactionsTax(operations);

        Assert.NotEmpty(results);
        Assert.Equal(operations.Count, results.Count);

        Assert.Equal(expectedTaxDueFirstSale, results[1].Tax);
        Assert.Equal(expectedTaxDueSecondSale, results[3].Tax);
    }

    [Fact] //cenario #9
    public void When_HasBigOperationMultiplePurchasesAndSales_Expected_VerifyTaxesSuccessfully()
    {
        var operations = new List<Operation>
        {
            new() { OperationDesc = "Buy", Quantity = 10, UnitCost = 5000 },
            new() { OperationDesc = "Sell", Quantity = 5, UnitCost = 4000 },
            new() { OperationDesc = "Buy", Quantity = 5, UnitCost = 15000 },
            new() { OperationDesc = "Buy", Quantity = 2, UnitCost = 4000 },
            new() { OperationDesc = "Buy", Quantity = 2, UnitCost = 23000 },
            new() { OperationDesc = "Sell", Quantity = 1, UnitCost = 20000 },
            new() { OperationDesc = "Sell", Quantity = 10, UnitCost = 12000 },
            new() { OperationDesc = "Sell", Quantity = 3, UnitCost = 15000 },
        };

        decimal expectedTaxDueSaleIndex1 = 0;
        decimal expectedLossesSaleIndex1 = -5000;

        decimal expectedTaxDueSaleIndex5 = 0;

        decimal expectedTaxDueSaleIndex6 = 1000;
        decimal expectedTaxDueSaleIndex7 = 2400;

        var operationService = new OperationService(taxService, logger);

        var results = operationService.CalculateTransactionsTax(operations);

        Assert.NotEmpty(results);
        Assert.Equal(operations.Count, results.Count);

        Assert.Equal(expectedTaxDueSaleIndex1, results[1].Tax);
        Assert.Equal(expectedLossesSaleIndex1, results[1].CurrentProfit);

        Assert.Equal(expectedTaxDueSaleIndex5, results[5].Tax);

        Assert.Equal(expectedTaxDueSaleIndex6, results[6].Tax);
        Assert.Equal(expectedTaxDueSaleIndex7, results[7].Tax);
    }
}
