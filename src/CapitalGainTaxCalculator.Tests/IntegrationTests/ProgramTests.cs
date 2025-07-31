using CapitalGainTaxCalculator.App;
using Xunit.Abstractions;

namespace CapitalGainTaxCalculator.Tests;

public class ProgramTests(ITestOutputHelper consoleTest)
{
    readonly ITestOutputHelper _consoleOutput = consoleTest;

    [Fact]
    public void When_ValidJsonInput_Expected_ValidJsonOutputNoTax()
    {
        string input = "[{\"operation\":\"buy\", \"unit-cost\":10.00, \"quantity\":100},{\"operation\":\"sell\", \"unit-cost\":15.00, \"quantity\":50},{\"operation\":\"sell\", \"unit-cost\":15.00, \"quantity\":50}]\n";

        var inputReader = new StringReader(input);
        Console.SetIn(inputReader);

        var expectedOutput = "[{\"tax\":0},{\"tax\":0},{\"tax\":0}]";

        using var outputWriter = new StringWriter();
        Console.SetOut(outputWriter);

        Program.Main([]);

        var results = outputWriter.ToString();

        _consoleOutput.WriteLine($"Saida do console: {results}");
        Assert.Contains(expectedOutput, results);

    }

    [Fact]
    public void When_ValidJsonInput_Expected_ValidJsonOutputWithTax()
    {
        string input = "[{\"operation\":\"buy\", \"unit-cost\":10.00, \"quantity\":10000},{\"operation\":\"sell\", \"unit-cost\":20.00, \"quantity\":5000},{\"operation\":\"sell\", \"unit-cost\":5.00, \"quantity\":5000}]\n";

        var inputReader = new StringReader(input);
        Console.SetIn(inputReader);

        var expectedOutput = "[{\"tax\":0},{\"tax\":10000.000},{\"tax\":0}]";

        using var outputWriter = new StringWriter();
        Console.SetOut(outputWriter);

        Program.Main([]);

        var results = outputWriter.ToString();

        _consoleOutput.WriteLine($"Saida do console: {results}");
        Assert.Contains(expectedOutput, results);
    }
}