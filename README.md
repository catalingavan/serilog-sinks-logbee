# Serilog.Sinks

A collection of Serilog sinks that write events to [logBee.net](https://logbee.net).

### [Serilog.Sinks.LogBee](src/Serilog.Sinks.LogBee#readme)

A Serilog sink used for non-web applications (Console applications, Worker services).

#### Basic usage

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
Log.CloseAndFlush();
```

#### Examples

- [ConsoleApp/Program1.cs](samples/Serilog.Sinks.LogBee_ConsoleApp/Program1.cs): Simple usage

- [ConsoleApp/Program2.cs](samples/Serilog.Sinks.LogBee_ConsoleApp/Program2.cs): Custom configuration

- [ConsoleApp/Program3.cs](samples/Serilog.Sinks.LogBee_ConsoleApp/Program3.cs) Using Microsoft.Extensions.Logging and Microsoft.Extensions.DependencyInjection

- [ConsoleApp/Program4.cs](samples/Serilog.Sinks.LogBee_ConsoleApp/Program4.cs): A console application which runs periodically

- [WorkerService](samples/Serilog.Sinks.LogBee_WorkerService/): A worker service application

### [Serilog.Sinks.LogBee.AspNetCore](src/Serilog.Sinks.LogBee.AspNetCore#readme)

A Serilog sink used for web applications.

#### Basic usage

```csharp
using Serilog;
using Serilog.Sinks.LogBee;
using Serilog.Sinks.LogBee.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();

builder.Services.AddSerilog((services, lc) => lc
    .WriteTo.LogBee(new LogBeeApiKey(
            "__LogBee.OrganizationId__",
            "__LogBee.ApplicationId__",
            "https://api.logbee.net"
        ),
        services
    ));

var app = builder.Build();

app.MapGet("/", (ILogger<Program> logger) =>
{
    logger.LogInformation("My favourite cartoon is {Name}", "Futurama");
    return "Hello";
});

// Important: register the LogBeeMiddleware() just before app.Run()
app.UseLogBeeMiddleware();

app.Run();
```

#### Examples

 - [WebApp](samples/Serilog.Sinks.LogBee_WebApp/)