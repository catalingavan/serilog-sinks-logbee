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
            if (httpContextLogger == null)
            {
                await _next(context);
                return;
            }

            if (context.Response.Body != null && context.Response.Body is MirrorStreamDecorator == false)
                context.Response.Body = new MirrorStreamDecorator(context.Response.Body);

            if (InternalHelpers.ShouldReadInputStream(context.Request.Headers, httpContextLogger.Config) &&
                httpContextLogger.Config.ShouldReadRequestBody(context.Request))
            {
                httpContextLogger.RequestBody = InternalHelpers.WrapInTryCatch(() =>
                {
                    var provider = new ReadInputStreamProvider();
                    return provider.ReadInputStream(context.Request);
                });
            }

            try
            {
                await _next(context);
            }
            finally
            {
                MirrorStreamDecorator? responseStream = GetResponseStream(context.Response);
                if (responseStream != null)
                {
                    httpContextLogger.ResponseContentLength = responseStream.MirrorStream.Length;

                    string? responseBody = InternalHelpers.ReadStreamAsString(responseStream.MirrorStream, responseStream.Encoding);
                    if (!string.IsNullOrEmpty(responseBody))
                        httpContextLogger.Logger.RequestInfoProvider.LogAsFile(responseBody, "Response.txt");

                    responseStream.MirrorStream.Dispose();
                }

                if (httpContextLogger.Config?.ShouldLogRequest(context) == true)
                {
                    await InternalHelpers.WrapInTryCatchAsync(async () =>
                    {
                        await httpContextLogger.Logger.FlushAsync().ConfigureAwait(false);
                    });
                }

                httpContextLogger.Dispose();
            }
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

    public static class LogBeeExtensions
    {
        public static IApplicationBuilder UseLogBeeMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LogBeeMiddleware>();
        }
    }
}
