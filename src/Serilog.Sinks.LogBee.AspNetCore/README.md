# Serilog.Sinks.LogBee.AspNetCore

A Serilog sink for web applications that writes events to [logBee.net](https://logbee.net).

LogBee sink keeps the events in the current `HttpContext` execution and flushes them automatically at the end of the request.

A fully working example can be found on [samples/Serilog.Sinks.LogBee_WebApp](/samples/Serilog.Sinks.LogBee_WebApp/).

### Basic usage

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

### Configuration

LogBee Sink supports multiple configuration properties that can be set using the `config` delegate parameter.

```csharp
builder.Services.AddSerilog((services, lc) => lc
    .WriteTo.LogBee(new LogBeeApiKey(
            "__LogBee.OrganizationId__",
            "__LogBee.ApplicationId__",
            "https://api.logbee.net"
        ),
        services,
        (config) =>
        {
            config.ShouldReadRequestHeader = (request, header) =>
            {
                if (string.Equals(header.Key, "X-ApiKey", StringComparison.OrdinalIgnoreCase))
                    return false;

                return true;
            };
        }
    ));
```
