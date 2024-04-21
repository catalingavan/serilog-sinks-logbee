using Serilog.Configuration;
using Serilog.Sinks.LogBee;
using Serilog.Sinks.LogBee.AspNetCore;
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
        IServiceProvider serviceProvider)
    {
        return LogBee(loggerConfiguration, apiKey, serviceProvider, (config) => { });
    }

    public static LoggerConfiguration LogBee(
        this LoggerSinkConfiguration loggerConfiguration,
        LogBeeApiKey apiKey,
        IServiceProvider serviceProvider,
        Action<LogBeeSinkAspNetCoreConfiguration> configureAction)
    {
        if (loggerConfiguration == null)
            throw new ArgumentNullException(nameof(loggerConfiguration));

        if (apiKey == null)
            throw new ArgumentNullException(nameof(apiKey));

        if (serviceProvider == null)
            throw new ArgumentNullException(nameof(serviceProvider));

        if (configureAction == null)
            throw new ArgumentNullException(nameof(configureAction));

        if (!apiKey.IsValid)
            return loggerConfiguration.Sink(new NullLogBeeSink());

        var config = new LogBeeSinkAspNetCoreConfiguration();
        configureAction.Invoke(config);

        var logBeeSink = new AspNetCoreLogBeeSink(apiKey, serviceProvider, config);

        return loggerConfiguration.Sink(
            logBeeSink
        );
    }
}
