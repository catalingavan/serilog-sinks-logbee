using Serilog.Sinks.LogBee.Rest;
using System.Text;
using System.Text.Json;

namespace Serilog.Sinks.LogBee
{
    internal static class InternalHelpers
    {
        public static string? GetMachineName()
        {
            string? name = null;

            try
            {
                name =
                    Environment.GetEnvironmentVariable("CUMPUTERNAME") ??
                    Environment.GetEnvironmentVariable("HOSTNAME") ??
                    System.Net.Dns.GetHostName();
            }
            catch
            {
                // ignored
            }

            return name;
        }

        public static CreateRequestLogPayload CreateRequestLogPayload(LoggerContext logger)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            var contextProvider = logger.GetContextProvider();
            var logBeeApiKey = logger.GetLogBeeApiKey();

            DateTime startedAt = contextProvider.GetStartedAt();
            var request = contextProvider.GetRequestProperties();
            var response = contextProvider.GetResponseProperties();
            int duration = Math.Max(0, Convert.ToInt32(Math.Round((DateTime.UtcNow - startedAt).TotalMilliseconds)));

            CreateRequestLogPayload payload = new CreateRequestLogPayload
            {
                StartedAt = startedAt,
                OrganizationId = logBeeApiKey.OrganizationId,
                ApplicationId = logBeeApiKey.ApplicationId,
                DurationInMilliseconds = duration,
                IntegrationClient = new CreateRequestLogPayload.IntegrationClientPayload
                {
                    Name = "Serilog.Sinks.LogBee",
                    Version = "0.0.1"
                },
                MachineName = GetMachineName(),
                HttpProperties = new CreateRequestLogPayload.HttpPropertiesPayload
                {
                    AbsoluteUri = request.AbsoluteUri.ToString(),
                    Method = request.Method,
                    RemoteAddress = request.RemoteAddress,
                    Request = new CreateRequestLogPayload.HttpPropertiesPayload.RequestPropertiesPayload
                    {
                        Headers = request.Headers,
                        FormData = request.FormData,
                        Claims = request.Claims,
                        Cookies = request.Cookies,
                        InputStream = request.RequestBody,
                    },
                    Response = new CreateRequestLogPayload.HttpPropertiesPayload.ResponsePropertiesPayload
                    {
                        StatusCode = response.StatusCode,
                        ContentLength = response.ContentLength ?? 0,
                        Headers = response.Headers
                    }
                },
                Logs = logger.GetLogs(),
                Exceptions = logger.GetExceptions()
            };

            return payload;
        }

        public static HttpContent CreateHttpContent(LoggerContext logger, CreateRequestLogPayload payload, LogBeeSinkConfiguration config)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            if (config == null)
                throw new ArgumentNullException(nameof(config));

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var files = logger.GetContextProvider().GetLoggedFiles();
            if (!files.Any())
                return content;

            MultipartFormDataContent form = new MultipartFormDataContent();
            form.Add(content, "RequestLog");

            foreach(var file in files)
            {
                if (file.FileSize > config.MaximumAllowedFileSizeInBytes)
                    continue;

                if (!System.IO.File.Exists(file.FilePath))
                    continue;

                form.Add(new ByteArrayContent(System.IO.File.ReadAllBytes(file.FilePath)), "Files", file.FileName);
            }

            return form;
        }

        public static void WrapInTryCatch(Action fn)
        {
            if (fn == null)
                throw new ArgumentNullException(nameof(fn));

            try
            {
                fn.Invoke();
            }
            catch (Exception)
            {
                // ignore
            }
        }

        public static T? WrapInTryCatch<T>(Func<T> fn)
        {
            if (fn == null)
                throw new ArgumentNullException(nameof(fn));

            try
            {
                return fn.Invoke();
            }
            catch (Exception)
            {
                // ignore
            }

            return default;
        }

        public static async Task WrapInTryCatchAsync(Func<Task> fn)
        {
            if (fn == null)
                throw new ArgumentNullException(nameof(fn));

            try
            {
                await fn.Invoke();
            }
            catch (Exception)
            {
                // ignore
            }
        }
    }
}
