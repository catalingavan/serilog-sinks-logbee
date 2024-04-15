namespace Serilog.Sinks.LogBee.AspNetCore
{
    public class LogBeeAspNetCoreConfiguration
    {
        public string[] ReadInputStreamContentTypes { get; set; }

        public LogBeeAspNetCoreConfiguration()
        {
            ReadInputStreamContentTypes = new[] { "application/javascript", "application/json", "application/xml", "text/plain", "text/xml", "text/html" };
        }
    }
}
