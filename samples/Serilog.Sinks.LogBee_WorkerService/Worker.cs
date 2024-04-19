using Serilog.Sinks.LogBee;
using Serilog.Sinks.LogBee.Context;

namespace Serilog.Sinks.LogBee_WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly LoggerContext2 _loggerContext;
        public Worker(
            ILogger<Worker> logger,
            LoggerContext2 loggerContext)
        {
            _logger = logger;
            _loggerContext = loggerContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int i = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                // _contextProvider.SetRequest(new RequestProperties(new Uri($"http://application/worker-service/execution-{++i}"), "GET"));

                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                await _loggerContext.FlushAsync();

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
