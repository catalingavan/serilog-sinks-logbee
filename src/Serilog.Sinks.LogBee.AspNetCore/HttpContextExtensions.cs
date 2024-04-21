using Microsoft.AspNetCore.Http;

namespace Serilog.Sinks.LogBee.AspNetCore
{
    public static class HttpContextExtensions
    {
        public static LoggerContext? GetLogBeeLoggerContext(this HttpContext context)
        {
            if (context == null)
                return null;

            if (context.Items.TryGetValue(Constants.HTTP_LOGGER_CONTEXT, out var value) && value is AspNetCoreLoggerContext loggerContext)
                return loggerContext;

            return null;
        }
    }
}
