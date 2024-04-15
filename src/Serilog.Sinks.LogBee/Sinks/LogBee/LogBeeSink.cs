using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.LogBee.RequestInfo;

namespace Serilog.Sinks.LogBee;

internal class LogBeeSink : ILogEventSink, IDisposable
{
    private readonly Logger _logger;
    public LogBeeSink(
        LogBeeApiKey apiKey,
        IRequestInfoProvider requestInfoProvider)
    {
        _logger = new Logger(apiKey, requestInfoProvider);
    }

    public void Emit(LogEvent logEvent)
    {
        _logger.Emit(logEvent);
    }

    public void Dispose()
    {
        _logger.Flush();
    }
}
