# Serilog.Sinks.LogBee

A Serilog sink that writes events to [logBee.net](https://logbee.net).

LogBee sink keeps the events in memory and commits them only when the logger is flushed.

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

// flush the logger so the events are sent to logBee.net
await Log.CloseAndFlushAsync();
```

Different use case examples can be found on the [Serilog.Sinks.LogBee_ConsoleApp](/samples/Serilog.Sinks.LogBee_ConsoleApp/) sample application.

### Advanced usage

Because logBee.net saves the events as "Requests", we can mock the http properties by providing a custom `ConsoleAppContextProvider`.

The provider allows us to specify the "Request" url as well as the "Response" status code.

<table><tr><td>
    <img alt="Console app request" src="https://github.com/logBee-net/serilog-sinks-logbee/assets/39127098/f34cf3b6-3bc2-4796-b6cc-1308dc9ae9c8" />
</td></tr></table>

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
            contextProvider,
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

try
{
    Log.Information("First log message from Serilog");
    throw new InvalidOperationException("Oops...");
}
catch (Exception ex)
{
    Log.Error(ex, "Unhandled exception");

    // because we had an error, we set the logBee.net StatusCode to 500
    // so we can easily identify failed executions
    contextProvider.SetResponse(new ResponseProperties(500));
}
finally
{
    await Log.CloseAndFlushAsync();
}
```