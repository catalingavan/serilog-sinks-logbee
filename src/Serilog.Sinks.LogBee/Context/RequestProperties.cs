using System;
using System.Collections.Generic;

namespace Serilog.Sinks.LogBee.Context
{
    public class RequestProperties
    {
        public Uri AbsoluteUri { get; }
        public string Method { get; }
        public Dictionary<string, string>? Headers { get; set; }
        public Dictionary<string, string>? Cookies { get; set; }
        public Dictionary<string, string>? FormData { get; set; }
        public Dictionary<string, string>? Claims { get; set; }
        public string? RequestBody { get; set; }
        public string? RemoteAddress { get; set; }

        public RequestProperties(Uri absoluteUri, string method)
        {
            if (absoluteUri == null)
                throw new ArgumentNullException(nameof(absoluteUri));

            if (!absoluteUri.IsAbsoluteUri)
                throw new ArgumentException($"{nameof(absoluteUri)} must be an absolute URI");

            if (string.IsNullOrWhiteSpace(method))
                throw new ArgumentNullException(nameof(method));

            AbsoluteUri = absoluteUri;
            Method = method;
        }
    }
}
