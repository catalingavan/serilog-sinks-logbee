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

            DateTime startedAt = requestInfoProvider.GetStartedAt();
            var request = requestInfoProvider.GetRequestProperties();
            var response = requestInfoProvider.GetResponseProperties();
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
                    AbsoluteUri = request.AbsoluteUri.ToString(),
                    Method = request.Method,
                    Request = new CreateRequestLogPayload.HttpPropertiesPayload.RequestPropertiesPayload
                    {
                        Headers = request.Headers,
                        FormData = request.FormData,
                        Claims = request.Claims,
                        Cookies = request.Cookies,
                        InputStream = request.InputStream
                    },
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
    }
}
