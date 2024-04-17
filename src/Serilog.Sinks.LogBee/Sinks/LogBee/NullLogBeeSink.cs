using Serilog.Core;
using Serilog.Events;

namespace Serilog.Sinks.LogBee;

internal class NullLogBeeSink : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        // do nothing
    }
}
