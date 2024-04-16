namespace Serilog.Sinks.LogBee.Context
{
    public interface IContextProvider : IDisposable
    {
        DateTime GetStartedAt();
        RequestProperties GetRequestProperties();
        ResponseProperties GetResponseProperties();
        List<LoggedFile> GetLoggedFiles();
        void LogFile(LoggedFile file);
    }
}
