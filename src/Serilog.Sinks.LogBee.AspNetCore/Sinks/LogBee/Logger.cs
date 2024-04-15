using Serilog.Events;
using Serilog.Sinks.LogBee.Rest;

namespace Serilog.Sinks.LogBee.AspNetCore
{
    internal class Logger
    {
        public LogBeeSinkConfiguration Config { get; init; }
        public List<CreateRequestLogPayload.LogMessagePayload> Logs { get; init; }
        public List<CreateRequestLogPayload.ExceptionPayload> Exceptions { get; init; }

        private readonly DateTime _createdAt;

        public Logger(LogBeeSinkConfiguration config)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            Logs = new();
            Exceptions = new();
            _createdAt = DateTime.UtcNow;
        }

        public void Emit(LogEvent logEvent)
        {
            int duration = Math.Max(0, Convert.ToInt32(Math.Round((DateTime.UtcNow - _createdAt).TotalMilliseconds)));

            Logs.Add(new CreateRequestLogPayload.LogMessagePayload
            {
                LogLevel = logEvent.Level.ToString(),
                Message = logEvent.RenderMessage(),
                MillisecondsSinceRequestStarted = duration
            });

            if (logEvent.Exception != null)
            {
                var type = logEvent.Exception.GetType();
                Exceptions.Add(new CreateRequestLogPayload.ExceptionPayload
                {
                    ExceptionType = type.FullName ?? type.Name,
                    ExceptionMessage = logEvent.Exception.Message
                });
            }
        }
    }
}
