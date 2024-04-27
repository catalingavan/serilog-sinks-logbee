# Serilog.Sinks.LogBee.AspNetCore

A Serilog sink for web applications that writes events to [logBee.net](https://logbee.net).

[Documentation](https://logbee.net/Docs/Integrations.Serilog-net.web-apps.index.html)

### Basic usage

```csharp
using Serilog;
using Serilog.Sinks.LogBee;
using Serilog.Sinks.LogBee.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();

builder.Services.AddSerilog((services, lc) => lc
    .WriteTo.LogBee(
        new LogBeeApiKey("_OrganizationId_", "_ApplicationId_", "https://api.logbee.net"),
        services
    ));

var app = builder.Build();

app.MapGet("/", (ILogger<Program> logger) =>
{
    logger.LogInformation("My favourite cartoon is {Name}", "Futurama");
    return "Hello";
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Important: register the LogBeeMiddleware() just before app.Run()
app.UseLogBeeMiddleware();

app.Run();
```

### Saving the logs

The ``Serilog.Sinks.LogBee.AspNetCore`` Sink stores all the events in the current http request (connection).

At the end of the request, the stored events are sent automatically to the logBee endpoint specified in the Sink configuration.

In addition to the log events, the LogBee Serilog Sink also collects all the HTTP properties of the current execution request.

**Important:** Make sure you register the LogBee middleware by calling ``app.UseLogBeeMiddleware()`` before the ``app.Run()``

### Configuration

Additional LogBee Serilog Sink configuration can be provided using the ``config`` parameter.

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog((services, lc) => lc
    .Enrich.WithCorrelationId()
    .WriteTo.LogBee(
        new LogBeeApiKey("_OrganizationId_", "_ApplicationId_", "https://api.logbee.net"),
        services,
        config =>
        {
            config.ShouldReadRequestHeader = (request, header) =>
            {
                // we don't want to log sensitive Header value
                if (string.Equals(header.Key, "X-Api-Key", StringComparison.OrdinalIgnoreCase))
                    return false;

                return true;
            };

            // handler used to determine if a Request should be saved to logBee endpoint or not
            config.ShouldLogRequest = (context) =>
            {
                if (string.Equals(context.Request.Path, "/status/healthcheck", StringComparison.OrdinalIgnoreCase)
                        && context.Response.StatusCode < 400)
                {
                    return false;
                }

                return true;
            };

            config.AppendExceptionDetails = (ex) =>
            {
                if (ex is NullReferenceException nullRefEx)
                    return "Don't forget to check for null references";

                return null;
            };

            config.RequestKeywords = (context) =>
            {
                var keywords = new List<string>();
                if (context.Items.TryGetValue("CorrelationIdEnricher+CorrelationId", out var value)
                        && value is string correlationId)
                {
                    // add the Serilog CorrelationId as a search keyword
                    keywords.Add(correlationId);
                }

                return keywords;
            };
        }
    ));
```

### Logging files

With ``Serilog.Sinks.LogBee.AspNetCore`` you can log string content as files.

In order to do so, you need to access the LoggerContext by using the ``HttpContext.GetLogBeeLoggerContext()`` extension method.

```csharp
public class HomeController : Controller
{
    public IActionResult Index()
    {
        var loggerContext = HttpContext.GetLogBeeLoggerContext();
        loggerContext?.LogAsFile(JsonSerializer.Serialize(new
        {
            eventCode = "AUTHORISATION",
            amount = new
            {
                currency = "USD",
                value = 12
            }
        }), "Event.json");

        return View();
    }
}
```

<table><tr><td>
    <img alt="Request Files tab" src="https://github.com/logBee-net/serilog-sinks-logbee/assets/39127098/d60dca14-91c7-4e4c-bda8-a42605a62d9c" />
</td></tr></table>

<table><tr><td>
    <img alt="Request File preview" src="https://github.com/logBee-net/serilog-sinks-logbee/assets/39127098/028c27fb-5cb8-4c90-99a4-3009e5f0197d" />
</td></tr></table>

### Examples

- [.NET web application](https://github.com/logBee-net/serilog-sinks-logbee/tree/feature/improvements/samples/Serilog.Sinks.LogBee_WebApp)
