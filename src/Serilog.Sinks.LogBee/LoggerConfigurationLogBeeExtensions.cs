using Serilog.Configuration;
using Serilog.Sinks.LogBee;
using Serilog.Sinks.LogBee.RequestInfo;

namespace Serilog;

/// <summary>
///     Adds the WriteTo.LogBee() extension method to <see cref="LoggerConfiguration" />.
/// </summary>
public static class LoggerConfigurationLogBeeExtensions
{
    public static LoggerConfiguration LogBee(
        this LoggerSinkConfiguration loggerConfiguration,
        LogBeeApiKey apiKey,
        IRequestInfoProvider? requestInfoProvider = null)
    {
        if (loggerConfiguration == null)
            throw new ArgumentNullException(nameof(loggerConfiguration));

        if (apiKey == null)
            throw new ArgumentNullException(nameof(apiKey));

        var logBeeSink = new LogBeeSink(
            apiKey,
            requestInfoProvider ?? new ConsoleAppRequestInfoProvider()
        );

        return loggerConfiguration.Sink(
            logBeeSink
        );
    }
}
