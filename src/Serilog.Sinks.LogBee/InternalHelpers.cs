using Serilog.Sinks.LogBee.RequestInfo;
using Serilog.Sinks.LogBee.Rest;
using System.Text;
using System.Text.Json;

namespace Serilog.Sinks.LogBee
{
    internal static class InternalHelpers
    {
        public static string? GetMachineName()
        {
            string? name = null;

            try
            {
                name =
                    Environment.GetEnvironmentVariable("CUMPUTERNAME") ??
                    Environment.GetEnvironmentVariable("HOSTNAME") ??
                    System.Net.Dns.GetHostName();
            }
            catch
            {
                // ignored
            }

            return name;
        }

        public static HttpContent CreateHttpContent(
            IRequestInfoProvider requestInfoProvider,
            LogBeeApiKey apiKey,
            List<CreateRequestLogPayload.LogMessagePayload> logs,
            List<CreateRequestLogPayload.ExceptionPayload> exceptions)
        {
            if (requestInfoProvider == null)
                throw new ArgumentNullException(nameof(requestInfoProvider));

            if (apiKey == null)
                throw new ArgumentNullException(nameof(apiKey));

            if (logs == null)
                throw new ArgumentNullException(nameof(logs));

            if (exceptions == null)
                throw new ArgumentNullException(nameof(exceptions));

            var payload = CreateRequestLogPayloadFactory.Create(requestInfoProvider, apiKey, logs, exceptions);
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var files = requestInfoProvider.GetFiles();
            if (!files.Any())
                return content;

            MultipartFormDataContent form = new MultipartFormDataContent();
            form.Add(content, "RequestLog");

            foreach(var file in files)
            {
                if (!System.IO.File.Exists(file.FilePath))
                    continue;

                form.Add(new ByteArrayContent(System.IO.File.ReadAllBytes(file.FilePath)), "Files", file.FileName);
            }

            return form;
        }
    }
}
