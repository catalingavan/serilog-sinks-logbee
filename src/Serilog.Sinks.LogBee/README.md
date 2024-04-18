# Serilog.Sinks.LogBee

A Serilog sink that writes events to [logbee.net](https://logbee.net).

Using the static ``Logger``:

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

// make sure to flush the logger so the events are sent to logBee
Log.CloseAndFlush();
```

Using a local ``Logger``:

```csharp
using Serilog;
using Serilog.Sinks.LogBee;

using(var log = new LoggerConfiguration()
    .WriteTo.LogBee(
        new LogBeeApiKey(
            "__LogBee.OrganizationId__",
            "__LogBee.ApplicationId__",
            "https://api.logbee.net"
        )
    )
    .CreateLogger())
{
    log.Information("First log message from Serilog");
}
```