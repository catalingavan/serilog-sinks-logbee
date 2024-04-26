using System;
using System.Collections.Generic;

namespace Serilog.Sinks.LogBee.ContextProperties
{
    public class RequestProperties
    {
        private const string DEFAULT_URI = "http://application";
        private const string DEFAULT_HTTP_METHOD = "GET";

        public Uri AbsoluteUri { get; }
        public string Method { get; }
        public Dictionary<string, string>? Headers { get; set; }
        public Dictionary<string, string>? Cookies { get; set; }
        public Dictionary<string, string>? FormData { get; set; }
        public Dictionary<string, string>? Claims { get; set; }
        public string? RequestBody { get; set; }
        public string? RemoteAddress { get; set; }

        public RequestProperties(string url = DEFAULT_URI, string method = DEFAULT_HTTP_METHOD)
        {
            if (string.IsNullOrWhiteSpace(method))
                throw new ArgumentNullException(nameof(method));

            AbsoluteUri = InternalHelpers.CreateLoggerContextUri(url);
            Method = method;
        }
    }
}
