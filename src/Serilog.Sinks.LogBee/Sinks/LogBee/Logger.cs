﻿using Serilog.Events;
using Serilog.Sinks.LogBee.RequestInfo;
using Serilog.Sinks.LogBee.Rest;

namespace Serilog.Sinks.LogBee
{
    internal class Logger
    {
        private readonly IRequestInfoProvider _requestInfoProvider;
        private readonly LogBeeApiKey _apiKey;
        private readonly List<CreateRequestLogPayload.LogMessagePayload> _logs;
        private readonly List<CreateRequestLogPayload.ExceptionPayload> _exceptions;
        public Logger(
            LogBeeApiKey apiKey,
            IRequestInfoProvider requestInfoProvider)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _requestInfoProvider = requestInfoProvider ?? throw new ArgumentNullException(nameof(requestInfoProvider));
            _logs = new();
            _exceptions = new();
        }

        public IRequestInfoProvider RequestInfoProvider => _requestInfoProvider;

        public void Emit(LogEvent logEvent)
        {
            if (logEvent == null)
                return;

            DateTime startedAt = _requestInfoProvider.GetStartedAt();
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

        public void Flush()
        {
            var content = InternalHelpers.CreateHttpContent(
                _requestInfoProvider,
                _apiKey,
                _logs,
                _exceptions
            );

            var client = new LogBeeRestClient(_apiKey);
            client.CreateRequestLog(content);
        }

        public async Task FlushAsync()
        {
            var content = InternalHelpers.CreateHttpContent(
                _requestInfoProvider,
                _apiKey,
                _logs,
                _exceptions
            );

            var client = new LogBeeRestClient(_apiKey);
            await client.CreateRequestLogAsync(content).ConfigureAwait(false);
        }
    }
}
