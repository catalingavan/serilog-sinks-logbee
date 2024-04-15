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
        string organizationId,
        string applicationId,
        string logBeeEndpoint,
        IServiceProvider serviceProvider)
    {
        if (loggerConfiguration == null)
            throw new ArgumentNullException(nameof(loggerConfiguration));

        if (serviceProvider == null)
            throw new ArgumentNullException(nameof(serviceProvider));

        var config = new Serilog.Sinks.LogBee.AspNetCore.LogBeeSinkConfiguration(organizationId, applicationId, logBeeEndpoint);

        var logBeeSink = new Serilog.Sinks.LogBee.AspNetCore.LogBeeSink(config, serviceProvider);

        return loggerConfiguration.Sink(
            logBeeSink
        );
    }
}
