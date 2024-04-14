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
            Uri absoluteUri,
            string httpMethod = "GET",
            RequestProperties? requestProperties = null)
        {
            if (absoluteUri == null)
                throw new ArgumentNullException(nameof(absoluteUri));

            if (!absoluteUri.IsAbsoluteUri)
                throw new ArgumentException($"{nameof(absoluteUri)} must be an absolute URI");

            if (string.IsNullOrWhiteSpace(httpMethod))
                throw new ArgumentNullException(nameof(httpMethod));

            _startedAt = DateTime.UtcNow;
            _absoluteUri = absoluteUri;
            _httpMethod = httpMethod;
            _requestProperties = requestProperties ?? new();
            _responseProperties = new ResponseProperties(200);
        }

        public DateTime StartedAt => _startedAt;
        public Uri AbsoluteUri => _absoluteUri;
        public string HttpMethod => _httpMethod;
        public RequestProperties RequestProperties => _requestProperties;
        public ResponseProperties ResponseProperties => _responseProperties;

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
