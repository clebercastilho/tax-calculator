using CapitalGainTaxCalculator.Domain.Contracts;
using CapitalGainTaxCalculator.Domain.Handlers;
using CapitalGainTaxCalculator.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace CapitalGainTaxCalculator.App;

public class Program
{
    public static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureLogger()
            .ConfigureServices((context, services) =>
            {
                services.AddTransient<IOperationService, OperationService>();
                services.AddTransient<ITaxService, TaxService>();
                services.AddTransient<AppService>();
            })
            .Build();

        var app = host.Services.GetRequiredService<AppService>();
        app.Run();
    }
}