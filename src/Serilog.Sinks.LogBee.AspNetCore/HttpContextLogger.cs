namespace Serilog.Sinks.LogBee.AspNetCore
{
    internal class HttpContextLogger
    {
        public Logger Logger { get; init; }
        public LogBeeSinkAspNetCoreConfiguration Config { get; init; }
        public string? RequestBody { get; set; }

        public HttpContextLogger(
            Logger logger,
            LogBeeSinkAspNetCoreConfiguration config)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Config = config ?? throw new ArgumentNullException(nameof(config));
        }
    }
}
