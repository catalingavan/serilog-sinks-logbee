namespace Serilog.Sinks.LogBee.Context
{
    public class ConsoleAppContextProvider : IContextProvider
    {
        private readonly DateTime _startedAt;
        private readonly List<LoggedFile> _loggedFiles;
        private RequestProperties _requestProperties;
        private ResponseProperties _responseProperties;
        public ConsoleAppContextProvider(
            string url = "http://application",
            string httpMethod = "GET")
        {
            if (string.IsNullOrWhiteSpace(httpMethod))
                throw new ArgumentNullException(nameof(httpMethod));

            Uri? absoluteUri = null;
            if (!string.IsNullOrWhiteSpace(url) && Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
                absoluteUri = uri.IsAbsoluteUri ? uri : new Uri(new Uri("http://application"), uri);

            if (absoluteUri == null)
                absoluteUri = new Uri("http://application", UriKind.Absolute);

            _startedAt = DateTime.UtcNow;
            _requestProperties = new RequestProperties(absoluteUri, httpMethod);
            _responseProperties = new(200);
            _loggedFiles = new();
        }

        public DateTime GetStartedAt() => _startedAt;
        public RequestProperties GetRequestProperties() => _requestProperties;
        public ResponseProperties GetResponseProperties() => _responseProperties;
        public List<LoggedFile> GetLoggedFiles() => _loggedFiles;
        public void SetRequest(RequestProperties requestProperties)
        {
            _requestProperties = requestProperties ?? throw new ArgumentNullException(nameof(requestProperties));
        }
        public void SetResponse(ResponseProperties responseProperties)
        {
            _responseProperties = responseProperties ?? throw new ArgumentNullException(nameof(responseProperties));
        }
        public void LogFile(LoggedFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            _loggedFiles.Add(file);
        }

        public void Dispose()
        {
            foreach (var file in _loggedFiles)
                file.Dispose();
        }
    }
}
