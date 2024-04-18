using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Serilog.Sinks.LogBee.Rest
{
    internal class LogBeeRestClient
    {
        private readonly HttpClient _httpClient;
        public LogBeeRestClient(
            HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public void CreateRequestLog(HttpContent content)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            using (content)
            {
                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/request-logs");
                httpRequest.Content = content;
                using HttpResponseMessage response = _httpClient.SendAsync(httpRequest).Result;
                if(!response.IsSuccessStatusCode)
                {
                    LogUnsuccessfulResponse(httpRequest, response);
                }
            }
        }

        public async Task CreateRequestLogAsync(HttpContent content)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            using (content)
            {
                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/request-logs");
                httpRequest.Content = content;
                using HttpResponseMessage response = await _httpClient.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    LogUnsuccessfulResponse(httpRequest, response);
                }
            }
        }

        private void LogUnsuccessfulResponse(HttpRequestMessage request, HttpResponseMessage response)
        {
            string responseAsStr = response.Content.ReadAsStringAsync().Result;
            var sb = new StringBuilder();
            sb.AppendLine($"Serilog.Sinks.LogBee: '{request.Method} {request.RequestUri}' responded with {(int)response.StatusCode} {response.StatusCode}");
            if (!string.IsNullOrWhiteSpace(responseAsStr))
            {
                responseAsStr = responseAsStr.Replace("{", "{{").Replace("}", "}}");
                sb.AppendLine(responseAsStr);
            }

            Serilog.Debugging.SelfLog.WriteLine(sb.ToString());
        }
    }
}
