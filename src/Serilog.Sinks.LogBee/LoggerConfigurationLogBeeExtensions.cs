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
        Action<LogBeeSinkConfiguration>? configureAction = null)
    {
        return LogBee(loggerConfiguration, apiKey, new NonWebLoggerContext(), configureAction);
    }

    public static LoggerConfiguration LogBee(
        this LoggerSinkConfiguration loggerConfiguration,
        LogBeeApiKey apiKey,
        LoggerContext loggerContext,
        Action<LogBeeSinkConfiguration>? configureAction = null)
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
        configureAction?.Invoke(config);

        loggerContext.Configure(apiKey, config);

        var logBeeSink = new LogBeeSink(loggerContext);

        return loggerConfiguration.Sink(
            logBeeSink
        );
    }
}
