using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Sinks.LogBee.AspNetCore;

internal class LogBeeSink : ILogEventSink
{
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
        if (httpContextAccessor == null)
            return;

        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null)
            return;

        HttpLoggerContainer? httpLoggerContainer = AspNetCoreHelpers.GetHttpLoggerContainer(httpContext);
        if (httpLoggerContainer == null)
        {
            var loggerContext = new LoggerContext(
                new HttpContextProvider(httpContext, _config),
                _apiKey,
                _config
            );
            httpLoggerContainer = new HttpLoggerContainer(loggerContext, _config);

            httpContext.Items.Add(Constants.HTTP_LOGGER_CONTAINER, httpLoggerContainer);
        }

        httpLoggerContainer.LoggerContext.Emit(logEvent);
    }
}
