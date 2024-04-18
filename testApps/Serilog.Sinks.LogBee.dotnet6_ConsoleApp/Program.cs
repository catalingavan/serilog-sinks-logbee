using Serilog;
using Serilog.Sinks.LogBee;
using Serilog.Sinks.LogBee.Context;

Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine(msg));

var contextProvider = new ConsoleAppContextProvider("http://application/console/main");

Log.Logger =
    new LoggerConfiguration()
        .WriteTo.Console()
        .WriteTo.LogBee(
            new LogBeeApiKey(
                "c6b66266-67ea-4719-aab4-809f5462c9a8",
                "c13a9c8e-6592-4693-a041-d13ccd31b5d8",
                "http://localhost:5265/"
            ),
            contextProvider,
            (config) =>
            {
                config.ClientTimeout = TimeSpan.FromSeconds(2);
            }
        )
        .CreateLogger();

try
{
    // Your program here...
    const string name = "Serilog";
    Log.Information("Hello, {Name}!", name);
    throw new InvalidOperationException("Oops...",
        new NotImplementedException("This is not implemented",
            new FileNotFoundException()
        )
    );
}
catch (Exception ex)
{
    Log.Error(ex, "Unhandled exception");
    contextProvider.SetResponse(new ResponseProperties(500));
}
finally
{
    await Log.CloseAndFlushAsync();
}

Log.Information("First log message from Serilog");

