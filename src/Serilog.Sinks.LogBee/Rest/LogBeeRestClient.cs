using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Serilog.Sinks.LogBee.Rest
{
    internal class LogBeeRestClient : ILogBeeRestClient
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        private readonly string _organizationId;
        private readonly string _applicationId;
        private readonly Uri _logBeeUri;
        public LogBeeRestClient(
            string organizationId,
            string applicationId,
            Uri logBeeUri)
        {
            if (string.IsNullOrWhiteSpace(organizationId))
                throw new ArgumentNullException(nameof(organizationId));

            if (string.IsNullOrWhiteSpace(applicationId))
                throw new ArgumentNullException(nameof(applicationId));

            if (logBeeUri == null)
                throw new ArgumentNullException(nameof(logBeeUri));

            if(!logBeeUri.IsAbsoluteUri)
                throw new ArgumentException($"{nameof(logBeeUri)} must be an absolute URI");

            _organizationId = organizationId;
            _applicationId = applicationId;
            _logBeeUri = logBeeUri;
        }

        public void CreateRequestLog(CreateRequestLogPayload payload)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            Uri uri = new Uri(_logBeeUri, "/request-logs");

            string requestPayload = JsonSerializer.Serialize(payload);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, uri);
            using HttpContent content = new StringContent(requestPayload, Encoding.UTF8, "application/json");
            httpRequest.Content = content;
            using HttpResponseMessage response = HttpClient.Send(httpRequest);
            
            Console.WriteLine($"Response: {response.StatusCode}");
        }
    }
}
