namespace Serilog.Sinks.LogBee.RequestInfo
{
    public interface IRequestInfoProvider
    {
        DateTime GetStartedAt();
        Uri GetAbsoluteUri();
        string GetHttpMethod();
        RequestProperties GetRequestProperties();
        ResponseProperties GetResponseProperties();
    }
}
