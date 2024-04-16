using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.LogBee.RequestInfo;

namespace Serilog.Sinks.LogBee;

internal class LogBeeSink : ILogEventSink, IDisposable
{
    private readonly Logger _logger;
    private readonly IRequestInfoProvider _requestInfoProvider;
    public LogBeeSink(
        LogBeeApiKey apiKey,
        IRequestInfoProvider requestInfoProvider)
    {
        _logger = new Logger(apiKey, requestInfoProvider);
        _requestInfoProvider = requestInfoProvider ?? throw new ArgumentNullException(nameof(requestInfoProvider));
    }

    public void Emit(LogEvent logEvent)
    {
        _logger.Emit(logEvent);
    }

    public void Dispose()
    {
        _logger.Flush();
        _requestInfoProvider.Dispose();
    }
}
