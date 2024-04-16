namespace Serilog.Sinks.LogBee.RequestInfo
{
    public class ConsoleAppRequestInfoProvider : IRequestInfoProvider
    {
        private readonly DateTime _startedAt;
        private RequestProperties _requestProperties;
        private ResponseProperties _responseProperties;
        private readonly List<FileLog> _files;
        public ConsoleAppRequestInfoProvider(
            string url = "http://application",
            string httpMethod = "GET")
        {
            Uri? absoluteUri = null;
            if(!string.IsNullOrWhiteSpace(url) && Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
                absoluteUri = uri.IsAbsoluteUri ? uri : new Uri(new Uri("http://application"), uri);

            if (absoluteUri == null)
                absoluteUri = new Uri("http://application", UriKind.Absolute);

            if (string.IsNullOrWhiteSpace(httpMethod))
                throw new ArgumentNullException(nameof(httpMethod));

            _startedAt = DateTime.UtcNow;
            _requestProperties = new(absoluteUri, httpMethod);
            _responseProperties = new ResponseProperties(200);
            _files = new();
        }

        public DateTime GetStartedAt() => _startedAt;
        public string? GetMachineName() => InternalHelpers.GetMachineName();
        public RequestProperties GetRequestProperties() => _requestProperties;
        public ResponseProperties GetResponseProperties() => _responseProperties;
        public List<LoggedFile> GetFiles() => _files.Select(p => new LoggedFile(p.TempFile.FileName, p.FileName, p.FileSize)).ToList();

        public void SetRequest(RequestProperties requestProperties)
        {
            _requestProperties = requestProperties ?? throw new ArgumentNullException(nameof(requestProperties));
        }
        public void SetResponse(ResponseProperties responseProperties)
        {
            _responseProperties = responseProperties ?? throw new ArgumentNullException(nameof(responseProperties));
        }
        public void LogAsFile(string contents, string? fileName = null)
        {
            FileLog? file = FileLog.Create(contents, fileName, 5 * 1024 * 1024);
            if (file != null)
                _files.Add(file);
        }

        public void Dispose()
        {
            foreach (var file in _files)
                file.Dispose();
        }
    }
}
