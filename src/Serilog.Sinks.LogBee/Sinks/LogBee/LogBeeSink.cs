using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.LogBee.Context;

namespace Serilog.Sinks.LogBee;

internal class LogBeeSink : ILogEventSink, IDisposable
{
    private readonly LoggerContext _loggerContext;
    public LogBeeSink(
        LogBeeApiKey apiKey,
        ContextProvider contextProvider,
        LogBeeSinkConfiguration config)
    {
        _loggerContext = new LoggerContext(contextProvider, apiKey, config);
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
