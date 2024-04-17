using Microsoft.AspNetCore.Http;
using Serilog.Sinks.LogBee.Context;
using System.Text;

namespace Serilog.Sinks.LogBee.AspNetCore
{
    internal class HttpContextProvider : ContextProvider
    {
        private readonly HttpContext _httpContext;
        private readonly LogBeeSinkAspNetCoreConfiguration _config;
        private readonly DateTime _startedAt;
        public HttpContextProvider(
            HttpContext httpContext,
            LogBeeSinkAspNetCoreConfiguration config)
        {
            _httpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _startedAt = DateTime.UtcNow;
        }

        public override DateTime GetStartedAt() => _startedAt;
        public override RequestProperties GetRequestProperties()
        {
            var request = _httpContext.Request;
            Uri requestUri = new Uri(GetDisplayUrl(_httpContext.Request), UriKind.Absolute);

            var result = new RequestProperties(requestUri, request.Method)
            {
                Headers = ReadRequestHeaders(request, request.Headers),
                Cookies = ReadRequestCookies(request, request.Cookies),
            };

            if (request.HasFormContentType)
                result.FormData = ReadRequestFormData(request, request.Form);

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

        private Dictionary<string, string> ReadRequestCookies(HttpRequest request, IRequestCookieCollection collection)
        {
            if (collection == null)
                return new Dictionary<string, string>();

            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (var keyValuePair in collection)
            {
                string key = keyValuePair.Key;
                if (string.IsNullOrWhiteSpace(key))
                    continue;

                if (_config.ShouldReadRequestCookie.Invoke(request, keyValuePair))
                {
                    result.TryAdd(key, keyValuePair.Value);
                }
            }

            return result;
        }

        private Dictionary<string, string> ReadRequestHeaders(HttpRequest request, IHeaderDictionary collection)
        {
            if (collection == null)
                return new Dictionary<string, string>();

            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (var keyValuePair in collection)
            {
                string key = keyValuePair.Key;
                if (string.IsNullOrWhiteSpace(key))
                    continue;

                if (_config.ShouldReadRequestHeader.Invoke(request, keyValuePair))
                {
                    string value = keyValuePair.Value.ToString();
                    result.TryAdd(key, value);
                }
            }

            return result;
        }

        private Dictionary<string, string> ReadRequestFormData(HttpRequest request, IFormCollection collection)
        {
            if (collection == null)
                return new Dictionary<string, string>();

            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (var keyValuePair in collection)
            {
                string key = keyValuePair.Key;
                if (string.IsNullOrWhiteSpace(key))
                    continue;

                if(_config.ShouldReadFormData.Invoke(request, keyValuePair))
                {
                    string value = keyValuePair.Value.ToString();
                    result.TryAdd(key, value);
                }
            }

            return result;
        }
    }
}
