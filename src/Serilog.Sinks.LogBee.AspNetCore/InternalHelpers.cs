using Microsoft.AspNetCore.Http;
using Serilog.Sinks.LogBee.RequestInfo;
using System.Text;

namespace Serilog.Sinks.LogBee.AspNetCore
{
    internal static class InternalHelpers
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

        public static T? WrapInTryCatch<T>(Func<T> fn)
        {
            if (fn == null)
                throw new ArgumentNullException(nameof(fn));

            try
            {
                return fn.Invoke();
            }
            catch (Exception)
            {
                // ignore
            }

            return default;
        }

        public static async Task WrapInTryCatchAsync(Func<Task> fn)
        {
            if (fn == null)
                throw new ArgumentNullException(nameof(fn));

            try
            {
                await fn.Invoke();
            }
            catch (Exception)
            {
                // ignore
            }
        }

        public static HttpContextLogger? GetHttpContextLogger(HttpContext context)
        {
            if (context.Items.TryGetValue(LogBeeSink.HTTP_CONTEXT_LOGGER, out var value))
                return value as HttpContextLogger;

            return null;
        }
    }
}
