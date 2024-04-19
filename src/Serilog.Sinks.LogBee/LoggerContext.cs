using Serilog.Events;
using Serilog.Sinks.LogBee.ContextProperties;
using Serilog.Sinks.LogBee.Exceptions;
using Serilog.Sinks.LogBee.Rest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Serilog.Sinks.LogBee
{
    public abstract class LoggerContext : IDisposable
    {
        private bool _isConfigured = false;

        private const string EXCEPTION_LOGGED_KEY = "Serilog.Sinks.LogBee.ExceptionLogged";
        private static readonly Regex FILE_NAME_REGEX = new Regex(@"[^a-zA-Z0-9_\-\+\. ]+", RegexOptions.Compiled);

        private Guid _loggerId;
        private List<CreateRequestLogPayload.LogMessagePayload> _logs = new();
        private List<CreateRequestLogPayload.ExceptionPayload> _exceptions = new();
        private List<LoggedFile> _loggedFiles = new();
        private DateTime _startedAt;

        private LogBeeApiKey _apiKey;
        private LogBeeSinkConfiguration _config;
        private HttpClient _httpClient;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public LoggerContext()
#pragma warning restore CS8618
        {
            InternalReset();
        }

        internal void Configure(
            LogBeeApiKey apiKey,
            LogBeeSinkConfiguration config)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _httpClient = CreateClient(apiKey, config);

            _isConfigured = true;
        }

        internal LogBeeApiKey ApiKey => _apiKey;
        internal DateTime StartedAt => _startedAt;
        internal List<CreateRequestLogPayload.LogMessagePayload> Logs => _logs.ToList();
        internal List<CreateRequestLogPayload.ExceptionPayload> Exceptions => _exceptions.ToList();
        internal List<LoggedFile> LoggedFiles => _loggedFiles.ToList();
        
        internal virtual IntegrationClient GetIntegrationClient() => InternalHelpers.IntegrationClient.Value;
        internal abstract RequestProperties GetRequestProperties();
        internal abstract ResponseProperties GetResponseProperties();
        internal abstract AuthenticatedUser? GetAuthenticatedUser();
        internal abstract List<string> GetKeywords();

        internal void Emit(LogEvent logEvent)
        {
            if (logEvent == null)
                return;

            string? category = null;
            if(logEvent.Properties.TryGetValue("SourceContext", out var value)
                && (value is ScalarValue scalarValue)
                && scalarValue.Value is string strValue)
            {
                category = strValue;
            }

            DateTime startedAt = _startedAt;
            int duration = Math.Max(0, Convert.ToInt32(Math.Round((DateTime.UtcNow - startedAt).TotalMilliseconds)));

            string message = logEvent.RenderMessage();

            if (logEvent.Exception != null)
            {
                var sb = new StringBuilder();
                sb.AppendLine(message);
                LogException(logEvent.Exception, sb);

                string? exceptionDetails = _config.AppendExceptionDetails?.Invoke(logEvent.Exception);
                if (!string.IsNullOrWhiteSpace(exceptionDetails))
                {
                    sb.AppendLine();
                    sb.AppendLine(exceptionDetails);
                }

                message = sb.ToString();
            }

            _logs.Add(new CreateRequestLogPayload.LogMessagePayload
            {
                CategoryName = category,
                LogLevel = GetLogLevel(logEvent.Level),
                Message = message,
                MillisecondsSinceRequestStarted = duration
            });
        }

        public void LogAsFile(string contents, string? fileName = null)
        {
            EnsureIsConfigured();

            if (string.IsNullOrEmpty(contents))
                return;

            if (contents.Length > _config.MaximumAllowedFileSizeInBytes)
                return;

            fileName = (fileName == null || string.IsNullOrWhiteSpace(fileName) ? "File" : fileName).Trim();
            fileName = FILE_NAME_REGEX.Replace(fileName, string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(fileName))
                fileName = "File";

            TemporaryFile? temporaryFile = null;
            try
            {
                temporaryFile = new TemporaryFile();
                File.WriteAllText(temporaryFile.FileName, contents);

                _loggedFiles.Add(new LoggedFile(temporaryFile.FileName, fileName, contents.Length));
            }
            catch (Exception)
            {
                if (temporaryFile != null)
                    temporaryFile.Dispose();
            }
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

        public void Flush()
        {
            EnsureIsConfigured();

            InternalHelpers.WrapInTryCatch(() =>
            {
                var payload = InternalHelpers.CreateRequestLogPayload(this);
                var httpContent = InternalHelpers.CreateHttpContent(this, payload);

                var client = new LogBeeRestClient(_httpClient);
                client.CreateRequestLog(httpContent);
            });

            InternalReset();
        }

        public async Task FlushAsync()
        {
            EnsureIsConfigured();

            await InternalHelpers.WrapInTryCatchAsync(async () =>
            {
                var payload = InternalHelpers.CreateRequestLogPayload(this);
                var httpContent = InternalHelpers.CreateHttpContent(this, payload);

                var client = new LogBeeRestClient(_httpClient);
                await client.CreateRequestLogAsync(httpContent).ConfigureAwait(false);
            });

            InternalReset();
        }

        protected void InternalReset()
        {
            if(_loggedFiles != null)
            {
                foreach (var file in _loggedFiles)
                    file.Dispose();
            }

            _loggerId = Guid.NewGuid();
            _startedAt = DateTime.UtcNow;
            _logs = new();
            _exceptions = new();
            _loggedFiles = new();
        }

        private HttpClient CreateClient(LogBeeApiKey apiKey, LogBeeSinkConfiguration config)
        {
            var client = new HttpClient();
            client.Timeout = config.ClientTimeout;
            client.BaseAddress = apiKey.LogBeeUri;

            return client;
        }

        private string GetLogLevel(LogEventLevel level)
        {
            switch (level)
            {
                case LogEventLevel.Verbose:
                    return "Trace";

                case LogEventLevel.Fatal:
                    return "Critical";
            }

            return level.ToString();
        }

        public virtual void Dispose()
        {
            if(_loggedFiles != null)
            {
                foreach (var file in _loggedFiles)
                    file.Dispose();
            }

            _httpClient?.Dispose();
        }

        private void EnsureIsConfigured()
        {
            if (!_isConfigured)
                throw new LoggerContextNotConfiguredException();
        }

    }
}
