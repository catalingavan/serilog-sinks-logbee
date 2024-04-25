using Microsoft.AspNetCore.Http;

namespace Serilog.Sinks.LogBee.AspNetCore
{
    public static class HttpContextExtensions
    {
        public static LoggerContext? GetLogBeeLoggerContext(this HttpContext context)
        {
            return GetOrCreateLoggerContext(context);
        }

        internal static AspNetCoreLoggerContext? GetOrCreateLoggerContext(this HttpContext context)
        {
            if (context == null)
                return null;

            if (context.Items.TryGetValue(Constants.HTTP_LOGGER_CONTEXT, out var value) && value is AspNetCoreLoggerContext contextValue)
                return contextValue;

            if (AspNetCoreLogBeeSink.SinkConfig != null)
            {
                var loggerContext = new AspNetCoreLoggerContext(context, AspNetCoreLogBeeSink.SinkConfig.Config, AspNetCoreLogBeeSink.SinkConfig.ApiKey);

                if (!context.Items.ContainsKey(Constants.HTTP_LOGGER_CONTEXT))
                    context.Items.Add(Constants.HTTP_LOGGER_CONTEXT, loggerContext);

                return loggerContext;
            }

            return null;
        }
    }
}
