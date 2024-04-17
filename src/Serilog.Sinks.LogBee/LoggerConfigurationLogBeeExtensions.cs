using Serilog.Configuration;
using Serilog.Sinks.LogBee;
using Serilog.Sinks.LogBee.Context;
using System;

namespace Serilog;

/// <summary>
///     Adds the WriteTo.LogBee() extension method to <see cref="LoggerConfiguration" />.
/// </summary>
public static class LoggerConfigurationLogBeeExtensions
{
    public static LoggerConfiguration LogBee(
        this LoggerSinkConfiguration loggerConfiguration,
        LogBeeApiKey apiKey,
        Action<LogBeeSinkConfiguration>? configAction = null)
    {
        return LogBee(loggerConfiguration, apiKey, new ConsoleAppContextProvider(), configAction);
    }

    public static LoggerConfiguration LogBee(
        this LoggerSinkConfiguration loggerConfiguration,
        LogBeeApiKey apiKey,
        ContextProvider contextProvider,
        Action<LogBeeSinkConfiguration>? configAction = null)
    {
        if (loggerConfiguration == null)
            throw new ArgumentNullException(nameof(loggerConfiguration));

        if (contextProvider == null)
            throw new ArgumentNullException(nameof(contextProvider));

        if (apiKey == null)
            throw new ArgumentNullException(nameof(apiKey));

        if(!apiKey.IsValid)
        {
            return loggerConfiguration.Sink(new NullLogBeeSink());
        }

        var config = new LogBeeSinkConfiguration();
        configAction?.Invoke(config);

        var logBeeSink = new LogBeeSink(
            apiKey,
            contextProvider,
            config
        );

        return loggerConfiguration.Sink(
            logBeeSink
        );
    }
}
