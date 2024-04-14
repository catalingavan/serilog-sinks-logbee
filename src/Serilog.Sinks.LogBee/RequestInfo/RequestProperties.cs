namespace Serilog.Sinks.LogBee.RequestInfo
{
    public class RequestProperties
    {
        public Dictionary<string, string>? Headers { get; set; }
        public Dictionary<string, string>? Cookies { get; set; }
        public Dictionary<string, string>? FormData { get; set; }
        public Dictionary<string, string>? ServerVariables { get; set; }
        public Dictionary<string, string>? Claims { get; set; }
        public string? InputStream { get; set; }
    }
}
