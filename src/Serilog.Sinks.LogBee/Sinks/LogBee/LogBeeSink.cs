using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.LogBee.Context;

namespace Serilog.Sinks.LogBee;

internal class LogBeeSink : ILogEventSink, IDisposable
{
    private readonly LoggerContext _loggerContext;
    public LogBeeSink(
        LogBeeApiKey apiKey,
        ContextProvider contextProvider)
    {
        _loggerContext = new LoggerContext(contextProvider, apiKey);
    }

    public void Emit(LogEvent logEvent)
    {
        _loggerContext.Emit(logEvent);
    }

    public void Dispose()
    {
        InternalHelpers.WrapInTryCatch(() => _loggerContext.Flush());
        
        _loggerContext.Dispose();
    }
}
