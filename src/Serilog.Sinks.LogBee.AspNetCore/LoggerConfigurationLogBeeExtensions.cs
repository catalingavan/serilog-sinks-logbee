using Serilog.Configuration;
using Serilog.Sinks.LogBee;
using Serilog.Sinks.LogBee.AspNetCore;

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
        Action<LogBeeSinkAspNetCoreConfiguration> configAction)
    {
        if (loggerConfiguration == null)
            throw new ArgumentNullException(nameof(loggerConfiguration));

        if (serviceProvider == null)
            throw new ArgumentNullException(nameof(serviceProvider));

        if (configAction == null)
            throw new ArgumentNullException(nameof(configAction));

        var config = new LogBeeSinkAspNetCoreConfiguration();
        configAction(config);

        var logBeeSink = new Serilog.Sinks.LogBee.AspNetCore.LogBeeSink(apiKey, serviceProvider, config);

        return loggerConfiguration.Sink(
            logBeeSink
        );
    }
}
