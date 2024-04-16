namespace Serilog.Sinks.LogBee.RequestInfo
{
    public interface IRequestInfoProvider
    {
        DateTime GetStartedAt();
        string? GetMachineName();
        RequestProperties GetRequestProperties();
        ResponseProperties GetResponseProperties();
    }
}
