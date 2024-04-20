using Serilog;
using Serilog.Sinks.LogBee;
using Serilog.Sinks.LogBee.AspNetCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();

builder.Services.AddSerilog((services, lc) => lc
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.WithCorrelationId()
    .WriteTo.LogBee(new LogBeeApiKey(
            builder.Configuration["LogBee.OrganizationId"]!,
            builder.Configuration["LogBee.ApplicationId"]!,
            builder.Configuration["LogBee.ApiUrl"]!
        ),
        services,
        (config) =>
        {
            config.AppendExceptionDetails = (ex) =>
            {
                if (ex is NullReferenceException nullRefEx)
                    return "Don't forget to check for null references";

                return null;
            };

            config.RequestKeywords = (context) =>
            {
                var keywords = new List<string>();
                if (context.Items.TryGetValue("CorrelationIdEnricher+CorrelationId", out var value) && value is string correlationId)
                {
                    if (!string.IsNullOrWhiteSpace(correlationId))
                        keywords.Add(correlationId);
                }

                return keywords;
            };
        }
    ));

var app = builder.Build();

app.MapGet("/", (ILogger<Program> logger) =>
{
    logger.LogInformation("My favourite cartoon is {Name}", "Futurama");

    return "Hello";
});

app.MapGet("/hello", (ILogger<Program> logger, HttpContext context) =>
{
    string name = "Serilog";
    logger.LogInformation("Hello, {Name}!", name);

    context.GetLogBeeLoggerContext()?.LogAsFile(JsonSerializer.Serialize(new { Hello = "World" }), "File.json");

    throw new NullReferenceException("Oops...");
});

app.UseLogBeeMiddleware();

app.Run();
