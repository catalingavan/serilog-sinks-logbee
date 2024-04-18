using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.LogBee;
using Serilog.Sinks.LogBee.Context;

Log.Logger =
    new LoggerConfiguration()
        .WriteTo.LogBee(
            new LogBeeApiKey(
                "0337cd29-a56e-42c1-a48a-e900f3116aa8",
                "4f729841-b103-460e-a87c-be6bd72f0cc9",
                "https://api.logbee.net/"
            ),
            new ConsoleAppContextProvider("http://application/dotnet6_consoleApp"),
            (config) =>
            {
                config.ClientTimeout = TimeSpan.FromSeconds(2);
                config.AppendExceptionDetails = (ex) =>
                {
                    if (ex is NullReferenceException nullRefEx)
                        return "Don't forget to check for null references";

                    return null;
                };
            }
        )
        .CreateLogger();

var services = new ServiceCollection();
services.AddLogging((builder) =>
{
    builder.AddSerilog();
});

var serviceProvider = services.BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
logger.LogWarning("Hello world");

// Log.Information("First log message from Serilog");

Log.CloseAndFlush();
