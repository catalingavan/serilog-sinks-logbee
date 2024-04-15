using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Sinks.LogBee.AspNetCore;

internal class LogBeeSink : ILogEventSink
{
    public const string HTTP_CONTEXT_LOGGER = "Serilog.LogBee.Logger";

    private readonly LogBeeApiKey _apiKey;
    private readonly IServiceProvider _serviceProvider;
    public LogBeeSink(
        LogBeeApiKey apiKey,
        IServiceProvider serviceProvider)
    {
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
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
            logger = new Logger(_apiKey, new HttpContextRequestInfoProvider(httpContextAccessor.HttpContext));
            httpContextAccessor.HttpContext.Items.Add(HTTP_CONTEXT_LOGGER, logger);
        }

        logger.Emit(logEvent);
    }
}
