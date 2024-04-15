namespace Serilog.Sinks.LogBee.RequestInfo
{
    public interface IRequestInfoProvider
    {
        DateTime GetStartedAt();
        RequestProperties GetRequestProperties();
        ResponseProperties GetResponseProperties();
    }
}
