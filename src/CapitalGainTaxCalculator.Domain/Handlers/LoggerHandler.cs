using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace CapitalGainTaxCalculator.Domain.Handlers;

public static class LoggerHandler
{
    public static IHostBuilder ConfigureLogger(this IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Information);
        });

        return hostBuilder;
    }
}