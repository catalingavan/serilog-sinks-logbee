namespace Serilog.Sinks.LogBee.Context
{
    public class ConsoleAppContextProvider : ContextProvider
    {
        private readonly DateTime _startedAt;
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
        }

        public override DateTime GetStartedAt() => _startedAt;
        public override RequestProperties GetRequestProperties() => _requestProperties;
        public override ResponseProperties GetResponseProperties() => _responseProperties;
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
