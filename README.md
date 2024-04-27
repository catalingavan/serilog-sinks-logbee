# Serilog.Sinks

A collection of Serilog sinks that write events to [logBee.net](https://logbee.net).

<table><tr><td>
    <img alt="Request view" src="https://github.com/logBee-net/serilog-sinks-logbee/assets/39127098/857e1e74-ef7a-428b-87a8-b4272b469241" />
</td></tr></table>

### [Serilog.Sinks.LogBee](src/Serilog.Sinks.LogBee#readme)

A Serilog sink used for non-web applications (Console applications, Worker services).

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.LogBee(new LogBeeApiKey("_OrganizationId_", "_ApplicationId_", "https://api.logbee.net"))
    .CreateLogger();

try
{
    Log.Information("Hello from {Name}!", "Serilog");
}
catch(Exception ex)
{
    Log.Error(ex, "Unhandled exception");
}
finally
{
    // send the events to logBee.net
    Log.CloseAndFlush();
}
```

### [Serilog.Sinks.LogBee.AspNetCore](src/Serilog.Sinks.LogBee.AspNetCore#readme)

A Serilog sink used for web applications.

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
