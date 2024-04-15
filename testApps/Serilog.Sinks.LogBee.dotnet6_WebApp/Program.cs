using Serilog;
using Serilog.Sinks.LogBee;
using Serilog.Sinks.LogBee.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();

builder.Services.AddSerilog((services, lc) => lc
    .ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.LogBee(new LogBeeApiKey(
            "c6b66266-67ea-4719-aab4-809f5462c9a8",
            "c13a9c8e-6592-4693-a041-d13ccd31b5d8",
            "http://localhost:5265/"
        ),
        services
    ));

var app = builder.Build();

app.MapGet("/", (ILoggerFactory loggerFactory) =>
{
    System.Threading.Thread.Sleep(500);

    var logger = loggerFactory.CreateLogger("MinimalApi");
    logger.LogWarning("Hey, this is a warn log");

    return "Hello World!";
});

app.UseLogBeeMiddleware();

app.Run();
