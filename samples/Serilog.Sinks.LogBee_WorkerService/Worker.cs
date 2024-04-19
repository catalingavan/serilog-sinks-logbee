using Serilog.Sinks.LogBee;
using Serilog.Sinks.LogBee_WorkerService.Services;

namespace Serilog.Sinks.LogBee_WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IFooService _fooService;
        private readonly NonWebLoggerContext _loggerContext;
        public Worker(
            ILogger<Worker> logger,
            IFooService fooService,
            NonWebLoggerContext loggerContext)
        {
            _logger = logger;
            _fooService = fooService;
            _loggerContext = loggerContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int i = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                _loggerContext.Reset($"http://application/worker-service/execution/{++i}", "GET");

                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                _loggerContext.LogAsFile("Content as file", "file.txt");

                _fooService.Foo();

                await _loggerContext.FlushAsync();

                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
