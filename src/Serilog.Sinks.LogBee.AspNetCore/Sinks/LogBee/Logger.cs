using Serilog.Events;
using Serilog.Sinks.LogBee.Rest;

namespace Serilog.Sinks.LogBee.AspNetCore
{
    internal class Logger
    {
        private readonly List<CreateRequestLogPayload.LogMessagePayload> _logs;
        private readonly List<CreateRequestLogPayload.ExceptionPayload> _exceptions;
        private readonly DateTime _createdAt;

        public Logger()
        {
            _logs = new();
            _exceptions = new();
            _createdAt = DateTime.UtcNow;
        }

        public void Emit(LogEvent logEvent)
        {
            int duration = Math.Max(0, Convert.ToInt32(Math.Round((DateTime.UtcNow - _createdAt).TotalMilliseconds)));

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
    }
}
