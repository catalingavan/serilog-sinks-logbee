using Serilog;
using Serilog.Sinks.LogBee;
using Serilog.Sinks.LogBee.Context;

var contextProvider = new ConsoleAppContextProvider();

Log.Logger =
    new LoggerConfiguration()
        .Enrich.WithProperty("Version", "1.2.3")
        .Enrich.FromLogContext()
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
                config.MaximumAllowedFileSizeInBytes = 5;
                config.True = false;
            }
        )
        .CreateLogger();

Log.Information("First log message from Serilog");

int i = 0;
while(i < 5)
{
    Log.Warning($"Test {i++}");
    System.Threading.Thread.Sleep(500);

    if (i >= 4)
    {
        contextProvider.LogAsFile("1234" + i, "Test1.txt");
        contextProvider.LogAsFile($"This is a file content {i}", "Test.txt");
    }
}

Log.CloseAndFlush();

Console.WriteLine("Finished");