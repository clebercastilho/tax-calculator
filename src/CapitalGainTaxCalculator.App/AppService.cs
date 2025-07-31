using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CapitalGainTaxCalculator.Domain.Contracts;
using CapitalGainTaxCalculator.Domain.Models;
using Microsoft.Extensions.Logging;

namespace CapitalGainTaxCalculator.App;

public class AppService(IOperationService operationService, ILogger<AppService> logger)
{
    readonly IOperationService _operationService = operationService;
    readonly ILogger<AppService> _logger = logger;

    readonly JsonSerializerOptions options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public void Run()
    {
        _logger.LogInformation("Enter JSON data (empty line to finish):");

        var jsonBuilder = new StringBuilder();
        string? jsonLine;

        while (!string.IsNullOrWhiteSpace(jsonLine = Console.ReadLine()))
        {
            jsonBuilder.AppendLine(jsonLine);
        }

        if (jsonBuilder.Length == 0)
        {
            _logger.LogInformation("No JSON data was provided");
            return;
        }

        string[] jsonList = jsonBuilder.ToString()
            .Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);

        foreach (var jsonProcess in jsonList)
        {
            var output = ProcessData(jsonProcess);
            Console.WriteLine(output);
        }
    }

    private string ProcessData(string jsonInput)
    {
        try
        {
            var operationsData = JsonSerializer.Deserialize<List<Operation>>(jsonInput, options);

            if (operationsData is null)
            {
                _logger.LogWarning("An error occurred while deserializing json data");
                return string.Empty;
            }

            var operationResults = _operationService.CalculateTransactionsTax(operationsData);
            return JsonSerializer.Serialize(operationResults);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception.Message);
            return string.Empty;
        }
    }
}