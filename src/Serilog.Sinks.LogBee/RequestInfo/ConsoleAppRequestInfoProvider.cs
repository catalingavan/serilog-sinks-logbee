namespace Serilog.Sinks.LogBee.RequestInfo
{
    public class ConsoleAppRequestInfoProvider : IRequestInfoProvider
    {
        private readonly DateTime _startedAt;
        private readonly Uri _absoluteUri;
        private readonly string _httpMethod;
        private RequestProperties _requestProperties;
        private ResponseProperties _responseProperties;
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
            _absoluteUri = absoluteUri;
            _httpMethod = httpMethod;
            _requestProperties = new();
            _responseProperties = new ResponseProperties(200);
        }

        public DateTime GetStartedAt() => _startedAt;
        public Uri GetAbsoluteUri() => _absoluteUri;
        public string GetHttpMethod() => _httpMethod;
        public RequestProperties GetRequestProperties() => _requestProperties;
        public ResponseProperties GetResponseProperties() => _responseProperties;

        public void SetRequest(RequestProperties requestProperties)
        {
            _requestProperties = requestProperties ?? throw new ArgumentNullException(nameof(requestProperties));
        }
        public void SetResponse(ResponseProperties responseProperties)
        {
            _responseProperties = responseProperties ?? throw new ArgumentNullException(nameof(responseProperties));
        }
    }
}
