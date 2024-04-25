using Serilog;
using Serilog.Sinks.LogBee;
using Serilog.Sinks.LogBee.AspNetCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();

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
            config.ShouldReadRequestHeader = (request, header) =>
            {
                if (string.Equals(header.Key, "X-Api-Key", StringComparison.OrdinalIgnoreCase))
                    return false;

                return true;
            };

            config.ShouldLogRequest = (context) =>
            {
                if(string.Equals(context.Request.Path, "/status/healthcheck", StringComparison.OrdinalIgnoreCase)
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

    var loggerContext = context.GetLogBeeLoggerContext();
    loggerContext?.LogAsFile(JsonSerializer.Serialize(new { Hello = "World" }), "File.json");
});

app.MapGet("/error", () =>
{
    throw new NullReferenceException("Oops...");
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseLogBeeMiddleware();

app.Run();
