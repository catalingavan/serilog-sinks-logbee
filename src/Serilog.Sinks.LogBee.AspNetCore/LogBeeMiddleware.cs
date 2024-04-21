using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Serilog.Sinks.LogBee.AspNetCore
{
    internal class LogBeeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LogBeeMiddleware> _logger;
        public LogBeeMiddleware(
            RequestDelegate next,
            ILogger<LogBeeMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            ExceptionDispatchInfo? ex = null;

            if (context.Response.Body != null && context.Response.Body is MirrorStreamDecorator == false)
                context.Response.Body = new MirrorStreamDecorator(context.Response.Body);

            try
            {
                await _next(context);
            }
            catch(Exception e)
            {
                ex = ExceptionDispatchInfo.Capture(e);
                throw;
            }
            finally
            {
                if (context.Items.TryGetValue(Constants.HTTP_LOGGER_CONTEXT, out var value) && value is AspNetCoreLoggerContext loggerContext)
                {
                    int statusCode = context.Response.StatusCode;
                    if (ex != null)
                    {
                        loggerContext.StatusCode = (int)HttpStatusCode.InternalServerError;
                        _logger.LogError(ex.SourceException, $"{context.Request.Method} {context.Request.Path} error");
                    }

                    await InternalHelpers.WrapInTryCatchAsync(async () =>
                    {
                        if(loggerContext.Config.ShouldLogRequest.Invoke(context))
                        {
                            await loggerContext.FlushAsync().ConfigureAwait(false);
                        }
                    });

                    loggerContext.Dispose();
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
