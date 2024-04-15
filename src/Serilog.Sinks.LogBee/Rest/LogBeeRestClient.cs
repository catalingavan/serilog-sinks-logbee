using System.Text;
using System.Text.Json;

namespace Serilog.Sinks.LogBee.Rest
{
    internal class LogBeeRestClient : ILogBeeRestClient
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        private readonly LogBeeApiKey _apiKey;
        public LogBeeRestClient(
            LogBeeApiKey apiKey)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        }

        public void CreateRequestLog(CreateRequestLogPayload payload)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            Uri uri = new Uri(_apiKey.LogBeeUri, "/request-logs");

            string requestPayload = JsonSerializer.Serialize(payload);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, uri);
            using HttpContent content = new StringContent(requestPayload, Encoding.UTF8, "application/json");
            httpRequest.Content = content;
            using HttpResponseMessage response = HttpClient.Send(httpRequest);
            
            Console.WriteLine($"Response: {response.StatusCode}");
        }

        public async Task CreateRequestLogAsync(CreateRequestLogPayload payload)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            Uri uri = new Uri(_apiKey.LogBeeUri, "/request-logs");

            string requestPayload = JsonSerializer.Serialize(payload);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, uri);
            using HttpContent content = new StringContent(requestPayload, Encoding.UTF8, "application/json");
            httpRequest.Content = content;
            using HttpResponseMessage response = await HttpClient.SendAsync(httpRequest);

            Console.WriteLine($"Response: {response.StatusCode}");
        }
    }
}
