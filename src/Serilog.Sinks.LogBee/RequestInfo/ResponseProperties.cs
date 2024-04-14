namespace Serilog.Sinks.LogBee.RequestInfo
{
    public class ResponseProperties
    {
        public int StatusCode { get; init; }
        public Dictionary<string, string>? Headers { get; init; }
        public long ContentLength { get; init; }

        public ResponseProperties(
            int statusCode,
            Dictionary<string, string>? headers = null,
            long? contentLength = null)
        {
            StatusCode = statusCode;
            Headers = headers;
            ContentLength = Math.Max(0, contentLength ?? 0);
        }
    }
}
