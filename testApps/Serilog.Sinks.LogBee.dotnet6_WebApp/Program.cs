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
            builder.Configuration["LogBee.OrganizationId"],
            builder.Configuration["LogBee.ApplicationId"],
            builder.Configuration["LogBee.ApiUrl"]
        ),
        services
    ));

var app = builder.Build();

app.MapGet("/", (ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("MinimalApi");
    logger.LogWarning("Hey, this is a warn message for second {Second}", DateTime.UtcNow.Second);

    return "Hello World!";
});

app.UseLogBeeMiddleware();

app.Run();
