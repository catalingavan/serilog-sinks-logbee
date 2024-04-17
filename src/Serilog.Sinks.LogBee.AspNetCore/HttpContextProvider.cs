using Microsoft.AspNetCore.Http;
using Serilog.Sinks.LogBee.Context;
using System.Security.Claims;
using System.Security.Principal;
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
                Claims = ReadClaims(),
                RemoteAddress = _httpContext.Connection.RemoteIpAddress?.ToString()
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
                Headers = ReadResponseHeaders(_httpContext, response.Headers)
            };

            HttpLoggerContainer? loggerContainer = AspNetCoreHelpers.GetHttpLoggerContainer(_httpContext);
            if (loggerContainer?.ResponseContentLength != null)
                response.ContentLength = loggerContainer.ResponseContentLength.Value;

            return result;
        }
        public override AuthenticatedUser? GetAuthenticatedUser()
        {
            if (_httpContext.User.Identity is ClaimsIdentity claimsIdentity == false)
                return null;

            string? name = claimsIdentity.Claims.FirstOrDefault(p => _config.UserNameClaims.Any(q => string.Equals(p.Type, q, StringComparison.OrdinalIgnoreCase)))?.Value;
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return new AuthenticatedUser(name);
        }
        public override IntegrationClient GetIntegrationClient() => AspNetCoreHelpers.IntegrationClient.Value;

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

        private Dictionary<string, string> ReadClaims()
        {
            IIdentity? identity = _httpContext.User.Identity;
            if (identity == null)
                return new();

            if (identity is ClaimsIdentity claimsIdentity == false)
                return new();

            var result = new Dictionary<string, string>();

            foreach (var claim in claimsIdentity.Claims)
            {
                if (string.IsNullOrWhiteSpace(claim.Type))
                    continue;

                if (_config.ShouldReadClaim.Invoke(_httpContext, claim))
                {
                    result.TryAdd(claim.Type, claim.Value);
                }
            }

            return result;
        }

        private Dictionary<string, string> ReadResponseHeaders(HttpContext context, IHeaderDictionary collection)
        {
            if (collection == null)
                return new Dictionary<string, string>();

            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (var keyValuePair in collection)
            {
                string key = keyValuePair.Key;
                if (string.IsNullOrWhiteSpace(key))
                    continue;

                if (_config.ShouldReadResponseHeader.Invoke(context, keyValuePair))
                {
                    string value = keyValuePair.Value.ToString();
                    result.TryAdd(key, value);
                }
            }

            return result;
        }

    }
}
