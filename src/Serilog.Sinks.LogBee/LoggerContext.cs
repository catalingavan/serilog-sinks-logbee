using Serilog.Events;
using Serilog.Sinks.LogBee.Context;
using Serilog.Sinks.LogBee.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Serilog.Sinks.LogBee
{
    internal class LoggerContext : IDisposable
    {
        private readonly ContextProvider _contextProvider;
        private readonly LogBeeApiKey _apiKey;
        private readonly LogBeeSinkConfiguration _config;
        private readonly List<CreateRequestLogPayload.LogMessagePayload> _logs;
        private readonly List<CreateRequestLogPayload.ExceptionPayload> _exceptions;
        public LoggerContext(
            ContextProvider contextProvider,
            LogBeeApiKey apiKey,
            LogBeeSinkConfiguration config)
        {
            _contextProvider = contextProvider ?? throw new ArgumentNullException(nameof(contextProvider));
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logs = new();
            _exceptions = new();
        }

        public void Emit(LogEvent logEvent)
        {
            if (logEvent == null)
                return;

            DateTime startedAt = _contextProvider.GetStartedAt();
            int duration = Math.Max(0, Convert.ToInt32(Math.Round((DateTime.UtcNow - startedAt).TotalMilliseconds)));

            _logs.Add(new CreateRequestLogPayload.LogMessagePayload
            {
                LogLevel = logEvent.Level.ToString(),
                Message = logEvent.RenderMessage(),
                MillisecondsSinceRequestStarted = duration
            });

            if (logEvent.Exception != null)
            {
                var type = logEvent.Exception.GetType();
                _exceptions.Add(new CreateRequestLogPayload.ExceptionPayload
                {
                    ExceptionType = type.FullName ?? type.Name,
                    ExceptionMessage = logEvent.Exception.Message
                });
            }
        }

        public List<CreateRequestLogPayload.LogMessagePayload> GetLogs() => _logs.ToList();
        public List<CreateRequestLogPayload.ExceptionPayload> GetExceptions() => _exceptions.ToList();
        public ContextProvider GetContextProvider() => _contextProvider;
        public LogBeeApiKey GetLogBeeApiKey() => _apiKey;

        public void Flush()
        {
            var payload = InternalHelpers.CreateRequestLogPayload(this);
            var httpContent = InternalHelpers.CreateHttpContent(this, payload, _config);

            var client = new LogBeeRestClient(_apiKey);
            client.CreateRequestLog(httpContent);
        }

        public async Task FlushAsync()
        {
            var payload = InternalHelpers.CreateRequestLogPayload(this);
            var httpContent = InternalHelpers.CreateHttpContent(this, payload, _config);

            var client = new LogBeeRestClient(_apiKey);
            await client.CreateRequestLogAsync(httpContent).ConfigureAwait(false);
        }

        public void Dispose()
        {
            _contextProvider.Dispose();
        }
    }
}
