using Microsoft.AspNetCore.Http;
using Serilog.Sinks.LogBee.Context;
using System.Text;

namespace Serilog.Sinks.LogBee.AspNetCore
{
    internal static class AspNetCoreHelpers
    {
        public static RequestProperties Create(HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            Uri requestUri = new Uri(GetDisplayUrl(request), UriKind.Absolute);

            var result = new RequestProperties(requestUri, request.Method)
            {
                Headers = ToDictionary(request.Headers),
                Cookies = ToDictionary(request.Cookies),
            };

            if (request.HasFormContentType)
                result.FormData = ToDictionary(request.Form);

            return result;
        }

        public static ResponseProperties Create(HttpResponse response)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            return new ResponseProperties(response.StatusCode)
            {
                Headers = ToDictionary(response.Headers)
            };
        }
        
        public static string GetDisplayUrl(HttpRequest request)
        {
            string value = request.Host.Value;
            string value2 = request.PathBase.Value ?? string.Empty;
            string value3 = request.Path.Value ?? string.Empty;
            string value4 = request.QueryString.Value ?? string.Empty;
            return new StringBuilder(request.Scheme.Length + "://".Length + value.Length + value2.Length + value3.Length + value4.Length).Append(request.Scheme).Append("://").Append(value)
                .Append(value2)
                .Append(value3)
                .Append(value4)
                .ToString();
        }

        public static Dictionary<string, string> ToDictionary(IRequestCookieCollection collection)
        {
            if (collection == null)
                return new Dictionary<string, string>();

            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (string key in collection.Keys)
            {
                if (string.IsNullOrWhiteSpace(key))
                    continue;
                
                string value = collection[key] ?? string.Empty;
                result.TryAdd(key, value);
            }

            return result;
        }

        public static Dictionary<string, string> ToDictionary(IHeaderDictionary collection)
        {
            if (collection == null)
                return new Dictionary<string, string>();

            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (string key in collection.Keys)
            {
                if (string.IsNullOrWhiteSpace(key))
                    continue;

                string value = collection[key].ToString();
                result.TryAdd(key, value);
            }

            return result;
        }

        public static Dictionary<string, string> ToDictionary(IFormCollection collection)
        {
            if (collection == null)
                return new Dictionary<string, string>();

            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (string key in collection.Keys)
            {
                if (string.IsNullOrWhiteSpace(key))
                    continue;

                string value = collection[key].ToString();
                result.TryAdd(key, value);
            }

            return result;
        }

        public static Dictionary<string, string> ToDictionary(IQueryCollection collection)
        {
            if (collection == null)
                return new Dictionary<string, string>();

            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (string key in collection.Keys)
            {
                if (string.IsNullOrWhiteSpace(key))
                    continue;

                string value = collection[key].ToString();
                result.TryAdd(key, value);
            }

            return result;
        }

        public static bool ShouldReadInputStream(IHeaderDictionary requestHeaders, LogBeeSinkAspNetCoreConfiguration config)
        {
            string? contentType = requestHeaders?.FirstOrDefault(p => string.Compare(p.Key, "Content-Type", StringComparison.OrdinalIgnoreCase) == 0).Value;
            if (string.IsNullOrEmpty(contentType))
                return false;

            contentType = contentType.Trim().ToLowerInvariant();

            return config.ReadRequestBodyContentTypes?.Any(p => contentType.Contains(p)) == true;
        }

        public static HttpLoggerContainer? GetHttpLoggerContainer(HttpContext context)
        {
            if (context.Items.TryGetValue(Constants.HTTP_CONTEXT_LOGGER_CONTAINER, out var value))
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
