using Microsoft.AspNetCore.Http;
using Serilog.Sinks.LogBee.RequestInfo;
using System.Text;

namespace Serilog.Sinks.LogBee.AspNetCore
{
    internal class HttpContextRequestInfoProvider : IRequestInfoProvider
    {
        private readonly HttpContext _httpContext;
        public HttpContextRequestInfoProvider(
            HttpContext httpContext)
        {
            _httpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
        }

        public DateTime StartedAt => DateTime.UtcNow;
        public Uri AbsoluteUri => new Uri(GetDisplayUrl(_httpContext.Request), UriKind.Absolute);
        public string HttpMethod => _httpContext.Request.Method;
        public RequestProperties RequestProperties => new();
        public ResponseProperties ResponseProperties => Create(_httpContext.Response);

        string GetDisplayUrl(HttpRequest request)
        {
            string value = request.Host.Value;
            string value2 = request.PathBase.Value;
            string value3 = request.Path.Value;
            string value4 = request.QueryString.Value;
            return new StringBuilder(request.Scheme.Length + "://".Length + value.Length + value2.Length + value3.Length + value4.Length).Append(request.Scheme).Append("://").Append(value)
                .Append(value2)
                .Append(value3)
                .Append(value4)
                .ToString();
        }

        private ResponseProperties Create(HttpResponse response)
        {
            return new ResponseProperties(response.StatusCode);
        }
    }
}
