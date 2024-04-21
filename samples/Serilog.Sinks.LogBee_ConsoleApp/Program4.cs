using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Sinks.LogBee;
using Serilog.Sinks.LogBee_ConsoleApp.Services;

namespace Serilog.Sinks.LogBee_ConsoleApp;

/// <summary>
/// A console application which runs periodically
/// If you use this scenario, maybe you can also consider the "samples/Serilog.Sinks.LogBee_WorkerService" example
/// </summary>
class Program4
{
    static async Task Main4(string[] args)
    {
        var loggerContext = new NonWebLoggerContext();

        Log.Logger =
            new LoggerConfiguration()
                .WriteTo.LogBee(
                    new LogBeeApiKey(
                        "0337cd29-a56e-42c1-a48a-e900f3116aa8",
                        "4f729841-b103-460e-a87c-be6bd72f0cc9",
                        "https://api.logbee.net/"
                    ),
                    loggerContext
                )
                .CreateLogger();

        var services = new ServiceCollection();
        services.AddLogging((builder) =>
        {
            builder.AddSerilog();
        });

        // we inject the loggerContext so we can access it later, throughout the code execution
        services.AddSingleton(loggerContext);
        
        services.AddTransient<IMainService, MainService>();

        var serviceProvider = services.BuildServiceProvider();

        int i = 0;
        while (true)
        {
            using (IServiceScope scope = serviceProvider.CreateScope())
            {
                Console.WriteLine($"Execution number {i} begin");

                loggerContext.Reset($"http://application/console-app-Program4/execution/{i}");

                var mainService = scope.ServiceProvider.GetRequiredService<IMainService>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program4>>();

                logger.LogInformation("Execution running on: {time}", DateTimeOffset.Now);

                try
                {
                    await mainService.ExecuteAsync();
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, "Error executing main service");
                }
                finally
                {
                    await loggerContext.FlushAsync();
                }
            }

            Console.WriteLine($"Execution number {i} completed");

            await Task.Delay(5000);
            i++;
        }
    }
}
