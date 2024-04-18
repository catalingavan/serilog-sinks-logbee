using System;
using System.Collections.Generic;

namespace Serilog.Sinks.LogBee.Rest
{
    internal class CreateRequestLogPayload
    {
        public IntegrationClientPayload? IntegrationClient { get; set; }
        public string OrganizationId { get; set; } = string.Empty;
        public string ApplicationId { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public int DurationInMilliseconds { get; set; }
        public string? MachineName { get; set; }
        public string? SessionId { get; set; }
        public bool IsNewSession { get; set; }
        public bool IsAuthenticated { get; set; }
        public UserPayload? User { get; set; }
        public HttpPropertiesPayload HttpProperties { get; set; } = default!;
        public List<LogMessagePayload>? Logs { get; set; }
        public List<ExceptionPayload>? Exceptions { get; set; }
        public List<string>? Keywords { get; set; }

        public class UserPayload
        {
            public string Name { get; set; } = default!;
        }

        public class IntegrationClientPayload
        {
            public string Name { get; set; } = default!;
            public string? Version { get; set; }
        }

        public class LogMessagePayload
        {
            public string? CategoryName { get; set; }
            public string LogLevel { get; set; } = default!;
            public string Message { get; set; } = default!;
            public int MillisecondsSinceRequestStarted { get; set; }
        }

        public class ExceptionPayload
        {
            public string ExceptionType { get; set; } = default!;
            public string ExceptionMessage { get; set; } = default!;
        }

        public class HttpPropertiesPayload
        {
            public string AbsoluteUri { get; set; } = default!;
            public string Method { get; set; } = default!;
            public string? RemoteAddress { get; set; }
            public RequestPropertiesPayload? Request { get; set; }
            public ResponsePropertiesPayload Response { get; set; } = default!;

            public class RequestPropertiesPayload
            {
                public Dictionary<string, string>? Headers { get; set; }
                public Dictionary<string, string>? Cookies { get; set; }
                public Dictionary<string, string>? FormData { get; set; }
                public Dictionary<string, string>? ServerVariables { get; set; }
                public Dictionary<string, string>? Claims { get; set; }
                public string? InputStream { get; set; }
            }

            public class ResponsePropertiesPayload
            {
                public int StatusCode { get; set; }
                public Dictionary<string, string>? Headers { get; set; }
                public long ContentLength { get; set; }
            }
        }
    }
}
