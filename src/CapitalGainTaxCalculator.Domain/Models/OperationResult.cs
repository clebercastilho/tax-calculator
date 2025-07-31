namespace CapitalGainTaxCalculator.Domain.Models;

public class OperationResult
{
    [JsonPropertyName("tax")]
    public decimal Tax { get; set; }

    [JsonIgnore]
    public decimal CurrentProfit { get; set; } = default!;

    [JsonIgnore]
    public decimal CurrentWeightedAverage { get; set; } = default!;

    [JsonIgnore]
    public string Message { get; set; } = default!;
}