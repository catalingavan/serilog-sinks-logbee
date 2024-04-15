using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Serilog.Sinks.LogBee.Rest;

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
            await _next(context);

            if (!(context.Items.TryGetValue(LogBeeSink.HTTP_CONTEXT_LOGGER, out var val) && val is Logger logger))
                return;

            var payload = CreateRequestLogPayloadFactory.Create(
                new HttpContextRequestInfoProvider(context),
                logger.Logs,
                logger.Exceptions
            );
            payload.OrganizationId = logger.Config.OrganizationId;
            payload.ApplicationId = logger.Config.ApplicationId;

            var client = new LogBeeRestClient(
                logger.Config.OrganizationId,
                logger.Config.ApplicationId,
                logger.Config.LogBeeUri
            );
            await client.CreateRequestLogAsync(payload);
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
