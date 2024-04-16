namespace Serilog.Sinks.LogBee.Rest
{
    internal class LogBeeRestClient
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        private readonly LogBeeApiKey _apiKey;
        public LogBeeRestClient(
            LogBeeApiKey apiKey)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        }

        public void CreateRequestLog(HttpContent content)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            using (content)
            {
                Uri uri = new Uri(_apiKey.LogBeeUri, "/request-logs");

                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, uri);
                httpRequest.Content = content;
                using HttpResponseMessage response = HttpClient.Send(httpRequest);

                Console.WriteLine($"Response: {response.StatusCode}");
            }
        }

        public async Task CreateRequestLogAsync(HttpContent content)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            using (content)
            {
                Uri uri = new Uri(_apiKey.LogBeeUri, "/request-logs");

                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, uri);
                httpRequest.Content = content;
                using HttpResponseMessage response = await HttpClient.SendAsync(httpRequest);

                Console.WriteLine($"Response: {response.StatusCode}");
            }
        }
    }
}
