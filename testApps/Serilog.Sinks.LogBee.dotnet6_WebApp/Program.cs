using Serilog;
using Serilog.Sinks.LogBee;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();

builder.Services.AddSerilog((services, lc) => lc
    .ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.LogBee(new LogBeeSinkConfiguration(
        "c6b66266-67ea-4719-aab4-809f5462c9a8",
        "c13a9c8e-6592-4693-a041-d13ccd31b5d8",
        "http://localhost:5265/",
        services
    )));

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
