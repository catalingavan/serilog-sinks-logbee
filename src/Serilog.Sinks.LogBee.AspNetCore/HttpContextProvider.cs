using Microsoft.AspNetCore.Http;
using Serilog.Sinks.LogBee.Context;
using System.Text;

namespace Serilog.Sinks.LogBee.AspNetCore
{
    internal class HttpContextProvider : ContextProvider
    {
        private readonly HttpContext _httpContext;
        private readonly DateTime _startedAt;
        public HttpContextProvider(
            HttpContext httpContext)
        {
            _httpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            _startedAt = DateTime.UtcNow;
        }

        public override DateTime GetStartedAt() => _startedAt;
        public override RequestProperties GetRequestProperties()
        {
            var request = _httpContext.Request;
            Uri requestUri = new Uri(GetDisplayUrl(_httpContext.Request), UriKind.Absolute);

            var result = new RequestProperties(requestUri, request.Method)
            {
                Headers = ToDictionary(request.Headers),
                Cookies = ToDictionary(request.Cookies),
            };

            if (request.HasFormContentType)
                result.FormData = ToDictionary(request.Form);

            HttpLoggerContainer? loggerContainer = AspNetCoreHelpers.GetHttpLoggerContainer(_httpContext);
            if (loggerContainer != null)
                result.RequestBody = loggerContainer.RequestBody;

            return result;
        }
        public override ResponseProperties GetResponseProperties()
        {
            var response = _httpContext.Response;

            var result = new ResponseProperties(response.StatusCode)
            {
                Headers = ToDictionary(response.Headers)
            };

            HttpLoggerContainer? loggerContainer = AspNetCoreHelpers.GetHttpLoggerContainer(_httpContext);
            if (loggerContainer?.ResponseContentLength != null)
                response.ContentLength = loggerContainer.ResponseContentLength.Value;

            return result;
        }

        private string GetDisplayUrl(HttpRequest request)
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

        private Dictionary<string, string> ToDictionary(IRequestCookieCollection collection)
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

        private Dictionary<string, string> ToDictionary(IHeaderDictionary collection)
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

        private Dictionary<string, string> ToDictionary(IFormCollection collection)
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
    }
}
