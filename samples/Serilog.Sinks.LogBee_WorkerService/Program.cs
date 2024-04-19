using Serilog;
using Serilog.Sinks.LogBee;
using Serilog.Sinks.LogBee_WorkerService;

var loggerContext = new NonWebLoggerContext("http://application/worker-service");

Log.Logger =
    new LoggerConfiguration()
        .WriteTo.LogBee(
            new LogBeeApiKey(
                "0337cd29-a56e-42c1-a48a-e900f3116aa8",
                "4f729841-b103-460e-a87c-be6bd72f0cc9",
                "https://api.logbee.net/"
            ),
            loggerContext
        )
        .CreateLogger();

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddSerilog();

builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<LoggerContext2>(loggerContext);

var host = builder.Build();
host.Run();
