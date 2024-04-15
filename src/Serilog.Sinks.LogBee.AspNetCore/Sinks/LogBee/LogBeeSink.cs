using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Sinks.LogBee.AspNetCore;

internal class LogBeeSink : ILogEventSink
{
    public const string HTTP_CONTEXT_LOGGER = "Serilog.LogBee.Logger";

    private readonly LogBeeApiKey _apiKey;
    private readonly IServiceProvider _serviceProvider;
    private readonly LogBeeSinkAspNetCoreConfiguration _config;
    public LogBeeSink(
        LogBeeApiKey apiKey,
        IServiceProvider serviceProvider,
        LogBeeSinkAspNetCoreConfiguration config)
    {
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public void Emit(LogEvent logEvent)
    {
        if(logEvent == null)
            throw new ArgumentNullException(nameof(logEvent));

        var httpContextAccessor = _serviceProvider.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
        if (httpContextAccessor == null || httpContextAccessor.HttpContext == null)
            return;

        HttpContextLogger httpContextLogger;
        if (httpContextAccessor.HttpContext.Items.ContainsKey(HTTP_CONTEXT_LOGGER))
        {
            httpContextLogger = (httpContextAccessor.HttpContext.Items[HTTP_CONTEXT_LOGGER] as HttpContextLogger)!;
        }
        else
        {
            var logger = new Logger(_apiKey, new HttpContextRequestInfoProvider(httpContextAccessor.HttpContext));
            httpContextLogger = new HttpContextLogger(logger, _config);

            httpContextAccessor.HttpContext.Items.Add(HTTP_CONTEXT_LOGGER, httpContextLogger);
        }

        httpContextLogger.Logger.Emit(logEvent);
    }
}
