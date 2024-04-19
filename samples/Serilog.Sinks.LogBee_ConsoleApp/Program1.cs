using Serilog;
using Serilog.Sinks.LogBee;
using Serilog.Sinks.LogBee.ContextProperties;

class Program1
{
    static async Task Main(string[] args)
    {
        var loggerContext = new NonWebLoggerContext("http://application/console-app-Program1");

        Log.Logger =
            new LoggerConfiguration()
                .WriteTo.LogBee(
                    new LogBeeApiKey(
                        "0337cd29-a56e-42c1-a48a-e900f3116aa8",
                        "4f729841-b103-460e-a87c-be6bd72f0cc9",
                        "https://api.logbee.net/"
                    ),
                    loggerContext,
                    (config) =>
                    {
                        config.AppendExceptionDetails = (ex) =>
                        {
                            if (ex is NullReferenceException nullRefEx)
                                return "Don't forget to check for null references";

                            return null;
                        };
                    }
                )
                .CreateLogger();

        loggerContext.LogAsFile("Content", "file.txt");

        try
        {
            const string name = "Serilog";
            Log.Information("Hello, {Name}!", name);

            throw new NullReferenceException("Oops...");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unhandled exception");
            loggerContext.SetResponseProperties(new ResponseProperties(500));
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}

