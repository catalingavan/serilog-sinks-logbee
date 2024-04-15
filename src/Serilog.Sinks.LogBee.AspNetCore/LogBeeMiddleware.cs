using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Serilog.Sinks.LogBee.AspNetCore
{
    internal class LogBeeMiddleware
    {
        private readonly RequestDelegate _next;
        public LogBeeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            HttpContextLogger? httpContextLogger = InternalHelpers.GetHttpContextLogger(context);

            if(httpContextLogger != null)
            {
                if (InternalHelpers.ShouldReadInputStream(context.Request.Headers, httpContextLogger.Config) &&
                    httpContextLogger.Config.ShouldReadRequestBody(context.Request))
                {
                    httpContextLogger.RequestBody = InternalHelpers.WrapInTryCatch(() =>
                    {
                        var provider = new ReadInputStreamProvider();
                        return provider.ReadInputStream(context.Request);
                    });
                }
            }

            try
            {
                await _next(context);
            }
            finally
            {
                if (httpContextLogger != null)
                {
                    await InternalHelpers.WrapInTryCatchAsync(async () =>
                    {
                        await httpContextLogger.Logger.FlushAsync().ConfigureAwait(false);
                    });
                }
            }
        }
    }

    public static class LogBeeExtensions
    {
        public static IApplicationBuilder UseLogBeeMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LogBeeMiddleware>();
        }
    }
}
