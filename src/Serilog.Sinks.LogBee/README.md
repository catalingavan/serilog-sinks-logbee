# Serilog.Sinks.LogBee

A Serilog sink that writes events to [logBee.net](https://logbee.net).

Different examples can be found on the [Serilog.Sinks.LogBee_ConsoleApp](/samples/Serilog.Sinks.LogBee_ConsoleApp/) sample application.

### Basic usage

```csharp
using Serilog;
using Serilog.Sinks.LogBee;

Log.Logger =
    new LoggerConfiguration()
        .WriteTo.LogBee(
            new LogBeeApiKey(
                "__LogBee.OrganizationId__",
                "__LogBee.ApplicationId__",
                "https://api.logbee.net"
            )
        )
        .CreateLogger();

Log.Information("First log message from Serilog");

// LogBee sink keeps the events in memory and commits them only when the logger is flushed
await Log.CloseAndFlushAsync();
```

### Advanced usage

logBee.net saves the log events under individual "Requests".

For non-web applications, a "Request" can be seen as individual "application executions". 

A `NonWebLoggerContext` can be used to configure the "Request" properties associated for the captured events.

```csharp
static async Task Main(string[] args)
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

    int executionCount = 0;
    while (true)
    {
        loggerContext.Reset($"http://application/execution/{executionCount}");

        Log.Information("First log message from Serilog");

        try
        {
            // execute some code

            if (executionCount % 2 == 1)
                throw new Exception("Oops, odd execution error");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing some code");
            loggerContext.SetResponseProperties(new ResponseProperties(500));
        }
        finally
        {
            await loggerContext.FlushAsync();
        }

        await Task.Delay(5000);
        executionCount++;
    }
}
```

<table><tr><td>
    <img alt="ConsoleApp request" src="https://github.com/logBee-net/serilog-sinks-logbee/assets/39127098/7eceaac8-d3f7-4380-8fd2-892d90d8af3f" />
</td></tr></table>
