using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net.Http;

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
            try
            {
                await _next(context);
            }
            finally
            {
                if (context.Items.TryGetValue(Constants.HTTP_LOGGER_CONTEXT, out var value) && value is AspNetCoreLoggerContext loggerContext)
                {
                    await InternalHelpers.WrapInTryCatchAsync(async () =>
                    {
                        await loggerContext.FlushAsync().ConfigureAwait(false);
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
