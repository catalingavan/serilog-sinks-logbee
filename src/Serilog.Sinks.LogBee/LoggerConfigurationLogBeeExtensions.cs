using Serilog.Configuration;
using Serilog.Sinks.LogBee;
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
        return LogBee(loggerConfiguration, apiKey, new NonWebLoggerContext(), configAction);
    }

    public static LoggerConfiguration LogBee(
        this LoggerSinkConfiguration loggerConfiguration,
        LogBeeApiKey apiKey,
        LoggerContext2 loggerContext,
        Action<LogBeeSinkConfiguration>? configAction = null)
    {
        if (loggerConfiguration == null)
            throw new ArgumentNullException(nameof(loggerConfiguration));

        if (loggerContext == null)
            throw new ArgumentNullException(nameof(loggerContext));

        if (apiKey == null)
            throw new ArgumentNullException(nameof(apiKey));

        if(!apiKey.IsValid)
            return loggerConfiguration.Sink(new NullLogBeeSink());

        var config = new LogBeeSinkConfiguration();
        configAction?.Invoke(config);

        loggerContext.Configure(apiKey, config);

        var logBeeSink = new LogBeeSink(loggerContext);

        return loggerConfiguration.Sink(
            logBeeSink
        );
    }
}
