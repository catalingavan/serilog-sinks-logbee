using Microsoft.AspNetCore.Http;
using Serilog.Sinks.LogBee.ContextProperties;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace Serilog.Sinks.LogBee.AspNetCore
{
    internal class AspNetCoreLoggerContext : LoggerContext
    {
        private readonly HttpContext _httpContext;
        private readonly LogBeeSinkAspNetCoreConfiguration _config;
        public AspNetCoreLoggerContext(
            HttpContext httpContext,
            LogBeeSinkAspNetCoreConfiguration config,
            LogBeeApiKey apiKey)
        {
            _httpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            _config = config ?? throw new ArgumentNullException(nameof(config));

            if (apiKey == null)
                throw new ArgumentNullException(nameof(apiKey));

            base.Configure(apiKey, config);
        }

        internal override RequestProperties GetRequestProperties()
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

            InternalHelpers.WrapInTryCatch(() =>
            {
                if (AspNetCoreHelpers.CanReadRequestBody(request.Headers, _config) && _config.ShouldReadRequestBody(request))
                {
                    var provider = new ReadInputStreamProvider();
                    result.RequestBody = provider.ReadInputStream(request);
                }
            });

            return result;

        }
        internal override ResponseProperties GetResponseProperties()
        {
            var response = _httpContext.Response;

            var result = new ResponseProperties(response.StatusCode)
            {
                Headers = ReadResponseHeaders(_httpContext, response.Headers)
            };

            InternalHelpers.WrapInTryCatch(() =>
            {
                var responseStream = GetResponseStream(_httpContext.Response);
                if (responseStream != null)
                {
                    if (AspNetCoreHelpers.CanReadResponseBody(response.Headers, _config) && _config.ShouldReadResponseBody(_httpContext))
                    {
                        string? responseBody = AspNetCoreHelpers.ReadStreamAsString(responseStream.MirrorStream, responseStream.Encoding);
                        string fileName = AspNetCoreHelpers.GetResponseFileName(response.Headers);
                        LogAsFile(responseBody ?? string.Empty, fileName);
                    }

                    result.ContentLength = responseStream.MirrorStream.Length;
                }
            });

            return result;
        }
        internal override AuthenticatedUser? GetAuthenticatedUser()
        {
            if (_httpContext.User.Identity is ClaimsIdentity claimsIdentity == false)
                return null;

            string? name = claimsIdentity.Claims.FirstOrDefault(p => _config.UserNameClaims.Any(q => string.Equals(p.Type, q, StringComparison.OrdinalIgnoreCase)))?.Value;
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return new AuthenticatedUser(name);
        }
        internal override IntegrationClient GetIntegrationClient() => AspNetCoreHelpers.IntegrationClient.Value;
        internal override List<string> GetKeywords() => _config.Keywords(_httpContext);

        public override void Dispose()
        {
            base.Dispose();

            var responseStream = GetResponseStream(_httpContext.Response);
            if (responseStream != null)
                responseStream.MirrorStream.Dispose();
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

        private MirrorStreamDecorator? GetResponseStream(HttpResponse response)
        {
            if (response.Body != null && response.Body is MirrorStreamDecorator stream)
            {
                if (!stream.MirrorStream.CanRead)
                    return null;

                return stream;
            }

            return null;
        }
    }
}
