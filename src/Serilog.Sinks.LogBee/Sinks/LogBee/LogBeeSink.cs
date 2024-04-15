using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.LogBee.RequestInfo;
using Serilog.Sinks.LogBee.Rest;

namespace Serilog.Sinks.LogBee;

internal class LogBeeSink : ILogEventSink, IDisposable
{
    private List<CreateRequestLogPayload.LogMessagePayload> _logs;
    private List<CreateRequestLogPayload.ExceptionPayload> _exceptions;

    private readonly LogBeeSinkConfiguration _config;
    private readonly ILogBeeRestClient _logBeeClient;
    private readonly Lazy<IRequestInfoProvider> _requestInfoProviderInstance;
    public LogBeeSink(LogBeeSinkConfiguration config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logBeeClient = new LogBeeRestClient(config.OrganizationId, config.ApplicationId, config.LogBeeUri);
        _requestInfoProviderInstance = new Lazy<IRequestInfoProvider>(() =>
        {
            if (_config.RequestInfoProvider != null)
                return _config.RequestInfoProvider;

            if (_config.ServiceProvider != null)
            {
                var provider = _config.ServiceProvider.GetService(typeof(IRequestInfoProvider)) as IRequestInfoProvider;
                if (provider != null)
                    return provider;
            }

            return new ConsoleAppRequestInfoProvider(new Uri("http://application"));
        });

        _logs = new();
        _exceptions = new();
    }

    public void Emit(LogEvent logEvent)
    {
        DateTime startedAt = _requestInfoProviderInstance.Value.StartedAt;
        int duration = Math.Max(0, Convert.ToInt32(Math.Round((DateTime.UtcNow - startedAt).TotalMilliseconds)));

        _logs.Add(new CreateRequestLogPayload.LogMessagePayload
        {
            LogLevel = logEvent.Level.ToString(),
            Message = logEvent.RenderMessage(),
            MillisecondsSinceRequestStarted = duration
        });

        if(logEvent.Exception != null)
        {
            var type = logEvent.Exception.GetType();
            _exceptions.Add(new CreateRequestLogPayload.ExceptionPayload
            {
                ExceptionType = type.FullName ?? type.Name,
                ExceptionMessage = logEvent.Exception.Message
            });
        }
    }

    public void Dispose()
    {
        var provider = _requestInfoProviderInstance.Value;
        var payload = CreateRequestLogPayloadFactory.Create(provider, _logs, _exceptions);
        payload.OrganizationId = _config.OrganizationId;
        payload.ApplicationId = _config.ApplicationId;

        _logBeeClient.CreateRequestLog(payload);

        _logs = new();
        _exceptions = new();
    }
}
