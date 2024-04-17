using Microsoft.AspNetCore.Http;
using System.Text;

namespace Serilog.Sinks.LogBee.AspNetCore
{
    internal static class AspNetCoreHelpers
    {
        public static bool CanReadRequestBody(IHeaderDictionary requestHeaders, LogBeeSinkAspNetCoreConfiguration config)
        {
            string? contentType = requestHeaders?.FirstOrDefault(p => string.Compare(p.Key, "Content-Type", StringComparison.OrdinalIgnoreCase) == 0).Value;
            if (string.IsNullOrEmpty(contentType))
                return false;

            contentType = contentType.Trim().ToLowerInvariant();

            return config.ReadRequestBodyContentTypes?.Any(p => contentType.Contains(p)) == true;
        }

        public static HttpLoggerContainer? GetHttpLoggerContainer(HttpContext context)
        {
            if (context.Items.TryGetValue(Constants.HTTP_LOGGER_CONTAINER, out var value))
                return value as HttpLoggerContainer;

            return null;
        }

        public static string? ReadStreamAsString(Stream stream, Encoding encoding)
        {
            if (stream == null || !stream.CanRead)
                return null;

            string? content = null;
            using (StreamReader reader = new StreamReader(stream, encoding, true))
            {
                stream.Position = 0;
                content = reader.ReadToEnd();
            }

            return content;
        }

        public static string GetResponseFileName(IHeaderDictionary responseHeaders)
        {
            string defaultValue = "Response.txt";
            if (responseHeaders == null)
                return defaultValue;

            string contentType = responseHeaders.FirstOrDefault(p => string.Compare(p.Key, "Content-Type", StringComparison.OrdinalIgnoreCase) == 0).Value.ToString();
            contentType = contentType.ToLowerInvariant();

            if (contentType.Contains("/json"))
                return "Response.json";

            if (contentType.Contains("/xml"))
                return "Response.xml";

            if (contentType.Contains("/html"))
                return "Response.html";

            return defaultValue;
        }
    }
}
