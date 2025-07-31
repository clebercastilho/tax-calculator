using System.Text.Json;
using CapitalGainTaxCalculator.App;
using CapitalGainTaxCalculator.Domain.Contracts;
using CapitalGainTaxCalculator.Domain.Models;
using CapitalGainTaxCalculator.Services;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.CrossPlatEngine.Client;
using NSubstitute;

namespace CapitalGainTaxCalculator.Tests;

public class AppServiceTests
{
    readonly IOperationService mockOperationService = Substitute.For<IOperationService>();
    readonly MockLogger<AppService> mockLogger = Substitute.For<MockLogger<AppService>>();

    [Fact]
    public void When_ValidJsonDataProvided_Expected_ValidJsonTaxOutput()
    {
        var jsonInput = JsonSerializer.Serialize(new List<Operation>
        {
            new() { OperationDesc = "Buy", Quantity = 2, UnitCost = 10 },
            new() { OperationDesc = "Sell", Quantity = 1, UnitCost = 10 },
            new() { OperationDesc = "Sell", Quantity = 1, UnitCost = 10 }
        });

        List<OperationResult> jsonOutput = [
            new OperationResult { Tax = 0 },
            new OperationResult { Tax = 0 },
            new OperationResult { Tax = 0 }
        ];

        mockOperationService.CalculateTransactionsTax(Arg.Any<List<Operation>>())
            .Returns(jsonOutput);

        StringReader inputReader = new($"{jsonInput}\n\n");

        var expectedOutput = JsonSerializer.Serialize(jsonOutput);

        StringWriter outputWriter = new();

        Console.SetIn(inputReader);
        Console.SetOut(outputWriter);

        new AppService(mockOperationService, mockLogger)
            .Run();


        Assert.Contains(expectedOutput, outputWriter.ToString());
    }

    [Fact]
    public void When_ValidMultiLineJsonDataProvided_Expected_ValidMultiLineJsonOutput()
    {
        var jsonInput = JsonSerializer.Serialize(new List<Operation>
        {
            new() { OperationDesc = "Buy", Quantity = 2, UnitCost = 10 },
            new() { OperationDesc = "Sell", Quantity = 2, UnitCost = 10 }
        });

        List<OperationResult> operationResult = [
            new OperationResult { Tax = 0 },
            new OperationResult { Tax = 0 },
            new OperationResult { Tax = 0 }
        ];

        mockOperationService.CalculateTransactionsTax(Arg.Any<List<Operation>>())
            .Returns(operationResult);

        //if input has two line json...
        StringReader inputReader = new($"{jsonInput}\n{jsonInput}\n\n");

        //...then output should have two line json
        var jsonOutput = JsonSerializer.Serialize(operationResult);
        var expectedOutput = $"{jsonOutput}\n{jsonOutput}\n";

        StringWriter outputWriter = new();
        Console.SetIn(inputReader);
        Console.SetOut(outputWriter);

        var appService = new AppService(mockOperationService, mockLogger);
        appService.Run();

        Assert.Contains(expectedOutput, outputWriter.ToString());
    }

    [Fact]
    public void When_EmptyInputIsProvided_Expected_LogInformationDisplayed()
    {
        StringReader inputReader = new($"\n");
        Console.SetIn(inputReader);

        string expectedMessage = "No JSON data was provided";

        var appService = new AppService(mockOperationService, mockLogger);
        appService.Run();

        mockLogger.Received(1).Log(LogLevel.Information, Arg.Is<string>(x => x.Contains(expectedMessage)));
    }

    [Fact]
    public void When_InvalidInputIsProvided_Expected_JsonDeserializeThrowsExceptionWithLogError()
    {
        StringReader inputReader = new("123\n"); //invalid input
        Console.SetIn(inputReader);

        StringWriter outputWriter = new();
        Console.SetOut(outputWriter);

        var appService = new AppService(mockOperationService, mockLogger);
        appService.Run();

        mockLogger.Received(1).Log(LogLevel.Error, Arg.Any<string>());
    }
}