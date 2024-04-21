# Serilog.Sinks.LogBee

A Serilog sink that writes events to [logBee.net](https://logbee.net).

LogBee sink keeps the events in memory and commits them only when the logger is flushed.

Different use-case examples can be found on [samples/Serilog.Sinks.LogBee_ConsoleApp](/samples/Serilog.Sinks.LogBee_ConsoleApp/).

Simple usage:

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

// flush the logger so the events are sent to logBee.net
await Log.CloseAndFlushAsync();
```

Advanced usage:

```csharp
using Serilog;
using Serilog.Sinks.LogBee;
using Serilog.Sinks.LogBee.Context;

// contextProvider can be used to configure additional properties that are sent to logBee.net
var contextProvider = new ConsoleAppContextProvider("http://application/console/main");

Log.Logger =
    new LoggerConfiguration()
        .WriteTo.LogBee(
            new LogBeeApiKey(
                "__LogBee.OrganizationId__",
                "__LogBee.ApplicationId__",
                "https://api.logbee.net"
            ),
            contextProvider
        )
        .CreateLogger();

try
{
    Log.Information("First log message from Serilog");
    throw new InvalidOperationException("Oops...");
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
```