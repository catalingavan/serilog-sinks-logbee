namespace Serilog.Sinks.LogBee.RequestInfo
{
    public interface IRequestInfoProvider
    {
        DateTime StartedAt { get; }
        Uri AbsoluteUri { get; }
        string HttpMethod { get; }
        RequestProperties RequestProperties { get; }
        ResponseProperties ResponseProperties { get; }
    }
}
