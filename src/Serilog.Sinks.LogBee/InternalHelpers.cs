using Serilog.Sinks.LogBee.Context;
using Serilog.Sinks.LogBee.Rest;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Serilog.Sinks.LogBee
{
    internal static class InternalHelpers
    {
        public static readonly Lazy<IntegrationClient> IntegrationClient =
            new Lazy<IntegrationClient>(() =>
            {
                return GetIntegrationClient(typeof(InternalHelpers).Assembly);
            });

        public static IntegrationClient GetIntegrationClient(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            var assemblyName = assembly.GetName();
            string? name = assemblyName.Name;
            if(string.IsNullOrWhiteSpace(name))
            {
                name = assemblyName.FullName;
                if (name.IndexOf(",") > -1)
                    name = name.Substring(0, name.IndexOf(","));
            }

            if (string.IsNullOrWhiteSpace(name))
                name = "Serilog.Sinks.LogBee";

            return new IntegrationClient(name, assemblyName.Version ?? new Version(0, 0, 1));
        }

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

            var authenticatedUser = contextProvider.GetAuthenticatedUser();
            var integrationClient = contextProvider.GetIntegrationClient();

            CreateRequestLogPayload payload = new CreateRequestLogPayload
            {
                StartedAt = startedAt,
                OrganizationId = logBeeApiKey.OrganizationId,
                ApplicationId = logBeeApiKey.ApplicationId,
                DurationInMilliseconds = duration,
                IntegrationClient = new CreateRequestLogPayload.IntegrationClientPayload
                {
                    Name = integrationClient.Name,
                    Version = integrationClient.Version.ToString()
                },
                MachineName = GetMachineName(),
                User = authenticatedUser == null ? null : new CreateRequestLogPayload.UserPayload
                {
                    Name = authenticatedUser.Name
                },
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
                Exceptions = logger.GetExceptions(),
                Keywords = logger.GetContextProvider().GetKeywords()
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

        public static CreateRequestLogPayload CreateRequestLogPayload(LoggerContext2 loggerContext)
        {
            if (loggerContext == null)
                throw new ArgumentNullException(nameof(loggerContext));

            var logBeeApiKey = loggerContext.ApiKey;

            DateTime startedAt = loggerContext.StartedAt;
            var request = new RequestProperties(new Uri("http://application"), "GET");
            var response = new ResponseProperties(200);
            int duration = Math.Max(0, Convert.ToInt32(Math.Round((DateTime.UtcNow - startedAt).TotalMilliseconds)));

            var authenticatedUser = new AuthenticatedUser("catalingavan");
            var integrationClient = IntegrationClient.Value;

            CreateRequestLogPayload payload = new CreateRequestLogPayload
            {
                StartedAt = startedAt,
                OrganizationId = logBeeApiKey.OrganizationId,
                ApplicationId = logBeeApiKey.ApplicationId,
                DurationInMilliseconds = duration,
                IntegrationClient = new CreateRequestLogPayload.IntegrationClientPayload
                {
                    Name = integrationClient.Name,
                    Version = integrationClient.Version.ToString()
                },
                MachineName = GetMachineName(),
                User = authenticatedUser == null ? null : new CreateRequestLogPayload.UserPayload
                {
                    Name = authenticatedUser.Name
                },
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
                Logs = loggerContext.Logs,
                Exceptions = loggerContext.Exceptions,
                Keywords = new()
            };

            return payload;
        }

        public static HttpContent CreateHttpContent(LoggerContext2 loggerContext, CreateRequestLogPayload payload)
        {
            if (loggerContext == null)
                throw new ArgumentNullException(nameof(loggerContext));

            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var files = loggerContext.LoggedFiles;
            if (!files.Any())
                return content;

            MultipartFormDataContent form = new MultipartFormDataContent();
            form.Add(content, "RequestLog");

            foreach (var file in files)
            {
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
