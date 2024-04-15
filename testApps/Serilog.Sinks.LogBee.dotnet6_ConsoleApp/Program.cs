using Serilog;
using Serilog.Sinks.LogBee;
using Serilog.Sinks.LogBee.RequestInfo;

Log.Logger =
    new LoggerConfiguration()
        .Enrich.WithProperty("Version", "1.2.3")
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.LogBee(new LogBeeSinkConfiguration(
            "c6b66266-67ea-4719-aab4-809f5462c9a8",
            "c13a9c8e-6592-4693-a041-d13ccd31b5d8",
            "http://localhost:5265/",
            new ConsoleAppRequestInfoProvider(
                "http://application/program/main",
                "POST"
            )
        ))
        .CreateLogger();

Log.Information("First log message from Serilog");

int i = 0;
while(i < 5)
{
    Log.Warning($"Test {i++}");
    System.Threading.Thread.Sleep(500);
}

Log.CloseAndFlush();

Console.WriteLine("Finished");