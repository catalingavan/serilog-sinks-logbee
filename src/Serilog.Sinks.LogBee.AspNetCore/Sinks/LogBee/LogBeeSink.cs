using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Sinks.LogBee.AspNetCore;

internal class LogBeeSink : ILogEventSink
{
    public const string HTTP_CONTEXT_LOGGER = "logBee.Logger";

    private readonly LogBeeSinkConfiguration _config;
    private readonly IServiceProvider _serviceProvider;
    public LogBeeSink(
        LogBeeSinkConfiguration config,
        IServiceProvider serviceProvider)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public void Emit(LogEvent logEvent)
    {
        if(logEvent == null)
            throw new ArgumentNullException(nameof(logEvent));

        var httpContextAccessor = _serviceProvider.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
        if (httpContextAccessor == null || httpContextAccessor.HttpContext == null)
            return;

        Logger logger;
        if (httpContextAccessor.HttpContext.Items.ContainsKey(HTTP_CONTEXT_LOGGER))
        {
            logger = (httpContextAccessor.HttpContext.Items[HTTP_CONTEXT_LOGGER] as Logger)!;
        }
        else
        {
            logger = new Logger(_config);
            httpContextAccessor.HttpContext.Items.Add(HTTP_CONTEXT_LOGGER, logger);
        }

        logger.Emit(logEvent);
    }
}
