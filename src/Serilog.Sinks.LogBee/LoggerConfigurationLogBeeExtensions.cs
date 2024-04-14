using Serilog.Configuration;
using Serilog.Sinks.LogBee;

namespace Serilog;

/// <summary>
///     Adds the WriteTo.LogBee() extension method to <see cref="LoggerConfiguration" />.
/// </summary>
public static class LoggerConfigurationLogBeeExtensions
{
    public static LoggerConfiguration LogBee(
            this LoggerSinkConfiguration loggerConfiguration,
            LogBeeSinkConfiguration config)
    {
        if (loggerConfiguration == null)
            throw new ArgumentNullException(nameof(loggerConfiguration));

        if (config == null)
            throw new ArgumentNullException(nameof(config));

        var logBeeSink = new LogBeeSink(config);

        return loggerConfiguration.Sink(
            logBeeSink
        );
    }
}
