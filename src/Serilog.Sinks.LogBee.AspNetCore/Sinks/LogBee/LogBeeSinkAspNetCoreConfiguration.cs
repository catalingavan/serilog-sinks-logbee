using Microsoft.AspNetCore.Http;

namespace Serilog.Sinks.LogBee.AspNetCore
{
    public class LogBeeSinkAspNetCoreConfiguration : LogBeeSinkConfiguration
    {
        public string[] ReadRequestBodyContentTypes { get; set; } = new[] { "application/javascript", "application/json", "application/xml", "text/plain", "text/xml", "text/html" };
        public Func<HttpRequest, bool> ShouldReadRequestBody { get; set; } = (request) => true;
        public Func<HttpContext, bool> ShouldLogRequest { get; set; } = (context) => true;
    }
}
