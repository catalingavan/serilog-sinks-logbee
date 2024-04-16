using Microsoft.AspNetCore.Http;
using Serilog.Sinks.LogBee.Context;

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
            var result = AspNetCoreHelpers.Create(_httpContext.Request);
            HttpLoggerContainer? httpLoggerContainer = AspNetCoreHelpers.GetHttpLoggerContainer(_httpContext);
            if(httpLoggerContainer != null)
            {
                result.RequestBody = httpLoggerContainer.RequestBody;
            }

            return result;
        }
        public override ResponseProperties GetResponseProperties()
        {
            var result = AspNetCoreHelpers.Create(_httpContext.Response);
            HttpLoggerContainer? httpLoggerContainer = AspNetCoreHelpers.GetHttpLoggerContainer(_httpContext);
            if(httpLoggerContainer?.ResponseContentLength != null)
            {
                result.ContentLength = httpLoggerContainer.ResponseContentLength.Value;
            }

            return result;
        }
    }
}
