using Serilog.Core;
using Serilog.Events;
using System;

namespace Serilog.Sinks.LogBee;

internal class LogBeeSink : ILogEventSink, IDisposable
{
    private readonly LoggerContext _loggerContext;
    public LogBeeSink(
        LoggerContext logerContext)
    {
        _loggerContext = logerContext ?? throw new ArgumentNullException(nameof(logerContext));
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
