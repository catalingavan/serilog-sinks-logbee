using Serilog.Sinks.LogBee;

namespace Serilog.Sinks.LogBee_ConsoleApp;

/// <summary>
/// Basic configuration
/// </summary>
class Program1
{
    static void Main(string[] args)
    {
        Log.Logger =
            new LoggerConfiguration()
                .WriteTo.LogBee(
                    new LogBeeApiKey(
                        "0337cd29-a56e-42c1-a48a-e900f3116aa8",
                        "4f729841-b103-460e-a87c-be6bd72f0cc9",
                        "https://api.logbee.net/"
                    )
                )
                .CreateLogger();

        try
        {
            string name = "Serilog";
            Log.Information("Hello, {Name}!", name);

            throw new NullReferenceException("Oops...");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unhandled exception");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
