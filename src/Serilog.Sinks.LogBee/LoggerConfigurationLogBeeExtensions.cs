using Serilog.Configuration;
using Serilog.Sinks.LogBee;
using Serilog.Sinks.LogBee.Context;

namespace Serilog;

/// <summary>
///     Adds the WriteTo.LogBee() extension method to <see cref="LoggerConfiguration" />.
/// </summary>
public static class LoggerConfigurationLogBeeExtensions
{
    public static LoggerConfiguration LogBee(
        this LoggerSinkConfiguration loggerConfiguration,
        LogBeeApiKey apiKey)
    {
        return LogBee(loggerConfiguration, apiKey, new ConsoleAppContextProvider());
    }

    public static LoggerConfiguration LogBee(
        this LoggerSinkConfiguration loggerConfiguration,
        LogBeeApiKey apiKey,
        ContextProvider contextProvider)
    {
        if (loggerConfiguration == null)
            throw new ArgumentNullException(nameof(loggerConfiguration));

        if (contextProvider == null)
            throw new ArgumentNullException(nameof(contextProvider));

        if (apiKey == null)
            throw new ArgumentNullException(nameof(apiKey));

        var logBeeSink = new LogBeeSink(
            apiKey,
            contextProvider
        );

        return loggerConfiguration.Sink(
            logBeeSink
        );
    }
}
