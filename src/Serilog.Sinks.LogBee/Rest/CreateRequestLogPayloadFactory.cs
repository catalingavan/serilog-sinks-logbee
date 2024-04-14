using Serilog.Sinks.LogBee.RequestInfo;

namespace Serilog.Sinks.LogBee.Rest
{
    internal static class CreateRequestLogPayloadFactory
    {
        public static CreateRequestLogPayload Create(
            IRequestInfoProvider requestInfoProvider,
            List<CreateRequestLogPayload.LogMessagePayload> logs,
            List<CreateRequestLogPayload.ExceptionPayload> exceptions)
        {
            if (requestInfoProvider == null)
                throw new ArgumentNullException(nameof(requestInfoProvider));

            if (logs == null)
                throw new ArgumentNullException(nameof(logs));

            if (exceptions == null)
                throw new ArgumentNullException(nameof(exceptions));

            DateTime startedAt = requestInfoProvider.StartedAt;
            var response = requestInfoProvider.ResponseProperties;
            int duration = Math.Max(0, Convert.ToInt32(Math.Round((DateTime.UtcNow - startedAt).TotalMilliseconds)));

            CreateRequestLogPayload payload = new CreateRequestLogPayload
            {
                StartedAt = startedAt,
                DurationInMilliseconds = duration,
                IntegrationClient = new CreateRequestLogPayload.IntegrationClientPayload
                {
                    Name = "Serilog.Sinks.LogBee",
                    Version = "0.0.1"
                },
                HttpProperties = new CreateRequestLogPayload.HttpPropertiesPayload
                {
                    AbsoluteUri = requestInfoProvider.AbsoluteUri.ToString(),
                    Method = requestInfoProvider.HttpMethod,
                    Request = ToPayload(requestInfoProvider.RequestProperties),
                    Response = new CreateRequestLogPayload.HttpPropertiesPayload.ResponsePropertiesPayload
                    {
                        StatusCode = response.StatusCode,
                        ContentLength = response.ContentLength,
                        Headers = response.Headers
                    }
                },
                Logs = logs,
                Exceptions = exceptions
            };

            return payload;
        }

        private static CreateRequestLogPayload.HttpPropertiesPayload.RequestPropertiesPayload ToPayload(RequestProperties requestProperties)
        {
            return new CreateRequestLogPayload.HttpPropertiesPayload.RequestPropertiesPayload
            {
                Headers = requestProperties.Headers,
                FormData = requestProperties.FormData,
                Claims = requestProperties.Claims,
                Cookies = requestProperties.Cookies,
                ServerVariables = requestProperties.ServerVariables,
                InputStream = requestProperties.InputStream
            };
        }
    }
}
