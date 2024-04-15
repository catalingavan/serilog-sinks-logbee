namespace Serilog.Sinks.LogBee.RequestInfo
{
    public class ResponseProperties
    {
        public int StatusCode { get; init; }
        public Dictionary<string, string>? Headers { get; set; }
        public long ContentLength { get; set; }

        public ResponseProperties(int statusCode)
        {
            StatusCode = statusCode;
        }
    }
}
