using Microsoft.AspNetCore.Http;
using Serilog.Sinks.LogBee.RequestInfo;

namespace Serilog.Sinks.LogBee.AspNetCore
{
    internal class HttpContextRequestInfoProvider : IRequestInfoProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public HttpContextRequestInfoProvider(
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public DateTime StartedAt => DateTime.UtcNow;
        public Uri AbsoluteUri => new Uri(new Uri("http://web-app.com"), _httpContextAccessor.HttpContext.Request.Path.ToString());
        public string HttpMethod => _httpContextAccessor.HttpContext.Request.Method;
        public RequestProperties RequestProperties => new();
        public ResponseProperties ResponseProperties => new();
    }
}
