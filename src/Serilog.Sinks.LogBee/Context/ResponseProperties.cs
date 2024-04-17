using System.Collections.Generic;

namespace Serilog.Sinks.LogBee.Context
{
    public class ResponseProperties
    {
        public int StatusCode { get; private set; }
        public Dictionary<string, string>? Headers { get; set; }
        public long? ContentLength { get; set; }

        public ResponseProperties(int statusCode)
        {
            StatusCode = statusCode;
        }
    }
}
