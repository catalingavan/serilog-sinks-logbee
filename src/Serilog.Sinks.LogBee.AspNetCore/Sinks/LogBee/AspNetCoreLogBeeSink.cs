using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;
using System;

namespace Serilog.Sinks.LogBee.AspNetCore;

internal class AspNetCoreLogBeeSink : ILogEventSink
{
    internal static SinkConfiguration? SinkConfig { get; private set; }

    private readonly LogBeeApiKey _apiKey;
    private readonly IServiceProvider _serviceProvider;
    private readonly LogBeeSinkAspNetCoreConfiguration _config;
    public AspNetCoreLogBeeSink(
        LogBeeApiKey apiKey,
        IServiceProvider serviceProvider,
        LogBeeSinkAspNetCoreConfiguration config)
    {
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _config = config ?? throw new ArgumentNullException(nameof(config));

        SinkConfig = new SinkConfiguration(apiKey, config);
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

        AspNetCoreLoggerContext? loggerContext = null;
        if (httpContext.Items.TryGetValue(Constants.HTTP_LOGGER_CONTEXT, out var value))
            loggerContext = value as AspNetCoreLoggerContext;

        if (loggerContext == null)
        {
            loggerContext = new AspNetCoreLoggerContext(httpContext, _config, _apiKey);
            if(!httpContext.Items.ContainsKey(Constants.HTTP_LOGGER_CONTEXT))
                httpContext.Items.Add(Constants.HTTP_LOGGER_CONTEXT, loggerContext);
        }

        loggerContext.Emit(logEvent);
    }

    internal class SinkConfiguration
    {
        public LogBeeApiKey ApiKey { get; private set; }
        public LogBeeSinkAspNetCoreConfiguration Config { get; private set; }

        public SinkConfiguration(
            LogBeeApiKey apiKey,
            LogBeeSinkAspNetCoreConfiguration config)
        {
            ApiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            Config = config ?? throw new ArgumentNullException(nameof(config));
        }
    }
}
