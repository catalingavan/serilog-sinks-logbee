using Microsoft.AspNetCore.Http;
using Serilog.Sinks.LogBee.RequestInfo;

namespace Serilog.Sinks.LogBee.AspNetCore
{
    internal class HttpContextRequestInfoProvider : IRequestInfoProvider
    {
        private readonly HttpContext _httpContext;
        private readonly DateTime _startedAt;
        public HttpContextRequestInfoProvider(
            HttpContext httpContext)
        {
            _httpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            _startedAt = DateTime.UtcNow;
        }

        public DateTime GetStartedAt() => _startedAt;
        public RequestProperties GetRequestProperties() => InternalHelpers.Create(_httpContext.Request);
        public ResponseProperties GetResponseProperties() => InternalHelpers.Create(_httpContext.Response);
    }
}
