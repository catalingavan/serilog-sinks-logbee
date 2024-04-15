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
            var test = new EnableBufferingReadInputStreamProvider();
            var inputStream = test.ReadInputStream(context.Request);

            await _next(context);

            if (!(context.Items.TryGetValue(LogBeeSink.HTTP_CONTEXT_LOGGER, out var val) && val is Logger logger))
                return;

            await logger.FlushAsync().ConfigureAwait(false);
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
