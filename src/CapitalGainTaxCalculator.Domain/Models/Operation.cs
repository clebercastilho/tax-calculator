namespace CapitalGainTaxCalculator.Domain.Models;

public class Operation
{
    [JsonPropertyName("operation")]
    public string OperationDesc { get; set; } = default!;

    [JsonIgnore]
    public OperationType OperationType => Convert(OperationDesc);

    [JsonPropertyName("unit-cost")]
    public decimal UnitCost { get; set; } = 0M;

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; } = 0;

    public static OperationType Convert(string operationDesc)
    {
        if (Enum.TryParse(operationDesc, ignoreCase:true, out OperationType result))
            return result;

        return OperationType.None;
    }
}
