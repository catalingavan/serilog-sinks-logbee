namespace Serilog.Sinks.LogBee
{
    public class LogBeeApiKey
    {
        internal bool IsValid { get; init; }

        public string OrganizationId { get; init; } = default!;
        public string ApplicationId { get; init; } = default!;
        public Uri LogBeeUri { get; init; } = default!;

        public LogBeeApiKey(string organizationId, string applicationId, string logBeeEndpoint)
        {
            if (string.IsNullOrWhiteSpace(organizationId))
                return;

            if (string.IsNullOrWhiteSpace(applicationId))
                return;

            if (string.IsNullOrWhiteSpace(logBeeEndpoint))
                return;

            if (!Uri.TryCreate(logBeeEndpoint, UriKind.Absolute, out var logBeeUri))
                return;

            OrganizationId = organizationId;
            ApplicationId = applicationId;
            LogBeeUri = logBeeUri;
            IsValid = true;
        }
    }
}
