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
            HttpLoggerContainer? httpLoggerContainer = AspNetCoreHelpers.GetHttpLoggerContainer(context);
            if (httpLoggerContainer == null)
            {
                await _next(context);
                return;
            }

            if (context.Response.Body != null && context.Response.Body is MirrorStreamDecorator == false)
                context.Response.Body = new MirrorStreamDecorator(context.Response.Body);

            if (AspNetCoreHelpers.CanReadRequestBody(context.Request.Headers, httpLoggerContainer.Config) &&
                httpLoggerContainer.Config.ShouldReadRequestBody(context.Request))
            {
                httpLoggerContainer.RequestBody = Serilog.Sinks.LogBee.InternalHelpers.WrapInTryCatch(() =>
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
                    httpLoggerContainer.ResponseContentLength = responseStream.MirrorStream.Length;

                    string? responseBody = AspNetCoreHelpers.ReadStreamAsString(responseStream.MirrorStream, responseStream.Encoding);
                    if (!string.IsNullOrEmpty(responseBody))
                    {
                        string fileName = AspNetCoreHelpers.GetResponseFileName(context.Response.Headers);
                        httpLoggerContainer.LoggerContext.GetContextProvider().LogAsFile(responseBody, fileName);
                    }

                    responseStream.MirrorStream.Dispose();
                }

                await InternalHelpers.WrapInTryCatchAsync(async () =>
                {
                    await httpLoggerContainer.LoggerContext.FlushAsync().ConfigureAwait(false);
                });

                httpLoggerContainer.Dispose();
            }
        }

        private void LogResponseBody(HttpContext context, HttpLoggerContainer loggerContainer)
        {
            MirrorStreamDecorator? responseStream = GetResponseStream(context.Response);
            if (responseStream == null)
                return;

            string? responseBody = AspNetCoreHelpers.ReadStreamAsString(responseStream.MirrorStream, responseStream.Encoding);
            if (!string.IsNullOrEmpty(responseBody))
            {
                string fileName = AspNetCoreHelpers.GetResponseFileName(context.Response.Headers);
                loggerContainer.LoggerContext.GetContextProvider().LogAsFile(responseBody, fileName);
            }

            responseStream.MirrorStream.Dispose();
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
