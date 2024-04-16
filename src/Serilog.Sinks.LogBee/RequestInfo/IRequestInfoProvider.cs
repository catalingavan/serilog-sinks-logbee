namespace Serilog.Sinks.LogBee.RequestInfo
{
    public interface IRequestInfoProvider : IDisposable
    {
        DateTime GetStartedAt();
        string? GetMachineName();
        RequestProperties GetRequestProperties();
        ResponseProperties GetResponseProperties();
        List<LoggedFile> GetFiles();
        void LogAsFile(string contents, string? fileName = null);
    }
}
