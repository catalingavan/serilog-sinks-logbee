namespace Serilog.Sinks.LogBee.AspNetCore
{
    internal class HttpLoggerContainer : IDisposable
    {
        public LoggerContext LoggerContext { get; init; }
        public LogBeeSinkAspNetCoreConfiguration Config { get; init; }
        public string? RequestBody { get; set; }
        public long? ResponseContentLength { get; set; }

        public HttpLoggerContainer(
            LoggerContext loggerContext,
            LogBeeSinkAspNetCoreConfiguration config)
        {
            LoggerContext = loggerContext ?? throw new ArgumentNullException(nameof(loggerContext));
            Config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public void Dispose()
        {
            LoggerContext.Dispose();
        }
    }
}
