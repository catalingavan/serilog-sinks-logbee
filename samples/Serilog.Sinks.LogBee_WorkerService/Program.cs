using Serilog;
using Serilog.Sinks.LogBee;
using Serilog.Sinks.LogBee_WorkerService;
using Serilog.Sinks.LogBee_WorkerService.Services;

var loggerContext = new NonWebLoggerContext();

Log.Logger =
    new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .WriteTo.LogBee(
            new LogBeeApiKey(
                "0337cd29-a56e-42c1-a48a-e900f3116aa8",
                "4f729841-b103-460e-a87c-be6bd72f0cc9",
                "https://api.logbee.net/"
            ),
            loggerContext,
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

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddSerilog();

builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton(loggerContext);
builder.Services.AddTransient<IFooService, FooService>();

var host = builder.Build();
host.Run();
