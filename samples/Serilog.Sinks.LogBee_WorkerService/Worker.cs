using Serilog.Sinks.LogBee.Context;

namespace Serilog.Sinks.LogBee_WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ContextProvider _contextProvider;
        public Worker(
            ILogger<Worker> logger,
            ContextProvider contextProvider)
        {
            _logger = logger;
            _contextProvider = contextProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int i = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                _contextProvider.SetRequest(new RequestProperties(new Uri($"http://application/worker-service/execution-{++i}"), "GET"));

                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                await _contextProvider.FlushAsync();

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
