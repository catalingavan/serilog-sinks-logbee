using Serilog.Events;
using Serilog.Sinks.LogBee.Context;
using Serilog.Sinks.LogBee.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Serilog.Sinks.LogBee
{
    internal class LoggerContext : IDisposable
    {
        private const string EXCEPTION_LOGGED_KEY = "Serilog.Sinks.LogBee.ExceptionLogged";

        private readonly Guid _loggerId;
        private readonly ContextProvider _contextProvider;
        private readonly LogBeeApiKey _apiKey;
        private readonly LogBeeSinkConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly List<CreateRequestLogPayload.LogMessagePayload> _logs;
        private readonly List<CreateRequestLogPayload.ExceptionPayload> _exceptions;
        public LoggerContext(
            ContextProvider contextProvider,
            LogBeeApiKey apiKey,
            LogBeeSinkConfiguration config)
        {
            _loggerId = Guid.NewGuid();
            _contextProvider = contextProvider ?? throw new ArgumentNullException(nameof(contextProvider));
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _httpClient = CreateClient(apiKey, config);
            _logs = new();
            _exceptions = new();
        }

        public void Emit(LogEvent logEvent)
        {
            if (logEvent == null)
                return;

            DateTime startedAt = _contextProvider.GetStartedAt();
            int duration = Math.Max(0, Convert.ToInt32(Math.Round((DateTime.UtcNow - startedAt).TotalMilliseconds)));

            string message = logEvent.RenderMessage();

            if(logEvent.Exception != null)
            {
                var sb = new StringBuilder();
                sb.AppendLine(message);
                LogException(logEvent.Exception, sb);

                string? exceptionDetails = _config.AppendExceptionDetails?.Invoke(logEvent.Exception);
                if(!string.IsNullOrWhiteSpace(exceptionDetails))
                {
                    sb.AppendLine();
                    sb.AppendLine(exceptionDetails);
                }

                message = sb.ToString();
            }

            _logs.Add(new CreateRequestLogPayload.LogMessagePayload
            {
                LogLevel = logEvent.Level.ToString(),
                Message = message,
                MillisecondsSinceRequestStarted = duration
            });
        }

        public List<CreateRequestLogPayload.LogMessagePayload> GetLogs() => _logs.ToList();
        public List<CreateRequestLogPayload.ExceptionPayload> GetExceptions() => _exceptions.ToList();
        public ContextProvider GetContextProvider() => _contextProvider;
        public LogBeeApiKey GetLogBeeApiKey() => _apiKey;

        public void Flush()
        {
            var payload = InternalHelpers.CreateRequestLogPayload(this);
            var httpContent = InternalHelpers.CreateHttpContent(this, payload, _config);

            var client = new LogBeeRestClient(_httpClient);
            client.CreateRequestLog(httpContent);
        }

        public async Task FlushAsync()
        {
            var payload = InternalHelpers.CreateRequestLogPayload(this);
            var httpContent = InternalHelpers.CreateHttpContent(this, payload, _config);

            var client = new LogBeeRestClient(_httpClient);
            await client.CreateRequestLogAsync(httpContent).ConfigureAwait(false);
        }

        public void Dispose()
        {
            _contextProvider.Dispose();
            _httpClient.Dispose();
        }

        private HttpClient CreateClient(LogBeeApiKey apiKey, LogBeeSinkConfiguration config)
        {
            var client = new HttpClient();
            client.Timeout = config.ClientTimeout;
            client.BaseAddress = apiKey.LogBeeUri;

            return client;
        }

        private void LogException(Exception ex, StringBuilder sb, string? header = null)
        {
            string id = $"{EXCEPTION_LOGGED_KEY}-{_loggerId}";

            bool alreadyLogged = ex.Data.Contains(id);
            if (alreadyLogged)
                return;

            var type = ex.GetType();

            _exceptions.Add(new CreateRequestLogPayload.ExceptionPayload
            {
                ExceptionType = type.FullName ?? type.Name,
                ExceptionMessage = ex.Message
            });

            ex.Data.Add(id, true);

            if (!string.IsNullOrEmpty(header))
                sb.AppendLine(header);

            sb.AppendLine(ex.ToString());

            Exception? innerException = ex.InnerException;
            while (innerException != null)
            {
                if (!innerException.Data.Contains(id))
                    innerException.Data.Add(id, true);

                innerException = innerException.InnerException;
            }

            Exception baseException = ex.GetBaseException();
            if (baseException != null)
            {
                LogException(baseException, sb, "Base Exception:");
            }
        }
    }
}
