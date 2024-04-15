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
        LogBeeApiKey apiKey,
        IServiceProvider serviceProvider)
    {
        if (loggerConfiguration == null)
            throw new ArgumentNullException(nameof(loggerConfiguration));

        if (serviceProvider == null)
            throw new ArgumentNullException(nameof(serviceProvider));

        var logBeeSink = new Serilog.Sinks.LogBee.AspNetCore.LogBeeSink(apiKey, serviceProvider);

        return loggerConfiguration.Sink(
            logBeeSink
        );
    }
}
