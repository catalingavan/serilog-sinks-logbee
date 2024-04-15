using Serilog.Sinks.LogBee.RequestInfo;

namespace Serilog.Sinks.LogBee;

internal class LogBeeSinkConfiguration
{
    public string OrganizationId { get; init; }
    public string ApplicationId { get; init; }
    public Uri LogBeeUri { get; init; }
    public IRequestInfoProvider? RequestInfoProvider { get; init; }
    public IServiceProvider? ServiceProvider { get; init; }

    public LogBeeSinkConfiguration(
        string organizationId,
        string applicationId,
        string logBeeEndpoint,
        IRequestInfoProvider requestInfoProvider) : this(organizationId, applicationId, logBeeEndpoint)
    {
        RequestInfoProvider = requestInfoProvider ?? throw new ArgumentNullException(nameof(logBeeEndpoint));
    }

    public LogBeeSinkConfiguration(
        string organizationId,
        string applicationId,
        string logBeeEndpoint,
        IServiceProvider serviceProvider) : this(organizationId, applicationId, logBeeEndpoint)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public LogBeeSinkConfiguration(
        string organizationId,
        string applicationId,
        string logBeeEndpoint)
    {
        if (string.IsNullOrWhiteSpace(organizationId))
            throw new ArgumentNullException(nameof(organizationId));

        if (string.IsNullOrWhiteSpace(applicationId))
            throw new ArgumentNullException(nameof(applicationId));

        if (string.IsNullOrWhiteSpace(logBeeEndpoint))
            throw new ArgumentNullException(nameof(logBeeEndpoint));

        if(!Uri.TryCreate(logBeeEndpoint, UriKind.Absolute, out var logBeeUri))
            throw new ArgumentException($"{nameof(logBeeEndpoint)} must be an absolute URI");

        OrganizationId = organizationId;
        ApplicationId = applicationId;
        LogBeeUri = logBeeUri;
    }
}
