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
            HttpLoggerContainer? loggerContainer = AspNetCoreHelpers.GetHttpLoggerContainer(context);
            if (loggerContainer == null)
            {
                await _next(context);
                return;
            }

            if (context.Response.Body != null && context.Response.Body is MirrorStreamDecorator == false)
                context.Response.Body = new MirrorStreamDecorator(context.Response.Body);

            TryReadRequestBody(context, loggerContainer);

            try
            {
                await _next(context);
            }
            finally
            {
                TryLogResponseBody(context, loggerContainer);
                TrySetKeywords(context, loggerContainer);

                await InternalHelpers.WrapInTryCatchAsync(async () =>
                {
                    await loggerContainer.LoggerContext.FlushAsync().ConfigureAwait(false);
                });

                loggerContainer.Dispose();
            }
        }

        private void TryReadRequestBody(HttpContext context, HttpLoggerContainer loggerContainer)
        {
            InternalHelpers.WrapInTryCatch(() =>
            {
                if (AspNetCoreHelpers.CanReadRequestBody(context.Request.Headers, loggerContainer.Config) &&
                    loggerContainer.Config.ShouldReadRequestBody(context.Request))
                {
                    var provider = new ReadInputStreamProvider();
                    loggerContainer.RequestBody = provider.ReadInputStream(context.Request);
                }
            });
        }

        private void TryLogResponseBody(HttpContext context, HttpLoggerContainer loggerContainer)
        {
            MirrorStreamDecorator? responseStream = GetResponseStream(context.Response);
            if (responseStream == null)
                return;

            InternalHelpers.WrapInTryCatch(() =>
            {
                if (AspNetCoreHelpers.CanReadResponseBody(context.Response.Headers, loggerContainer.Config) &&
                    loggerContainer.Config.ShouldReadResponseBody(context))
                {
                    string? responseBody = AspNetCoreHelpers.ReadStreamAsString(responseStream.MirrorStream, responseStream.Encoding);
                    if (!string.IsNullOrEmpty(responseBody))
                    {
                        string fileName = AspNetCoreHelpers.GetResponseFileName(context.Response.Headers);
                        loggerContainer.LoggerContext.GetContextProvider().LogAsFile(responseBody, fileName);
                    }
                }
            });

            responseStream.MirrorStream.Dispose();
        }

        private void TrySetKeywords(HttpContext context, HttpLoggerContainer loggerContainer)
        {
            InternalHelpers.WrapInTryCatch(() =>
            {
                var keywords = loggerContainer.Config.Keywords(context) ?? new();
                loggerContainer.LoggerContext.GetContextProvider().SetKeywords(keywords);
            });
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
