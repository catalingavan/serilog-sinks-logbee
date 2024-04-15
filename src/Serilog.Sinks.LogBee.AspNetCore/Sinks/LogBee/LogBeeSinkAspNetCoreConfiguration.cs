using Microsoft.AspNetCore.Http;

namespace Serilog.Sinks.LogBee.AspNetCore
{
    public class LogBeeSinkAspNetCoreConfiguration
    {
        public string[] ReadRequestBodyContentTypes { get; set; }
        public Func<HttpRequest, bool> ShouldReadRequestBody { get; set; }

        public LogBeeSinkAspNetCoreConfiguration()
        {
            ReadRequestBodyContentTypes = new[] { "application/javascript", "application/json", "application/xml", "text/plain", "text/xml", "text/html" };
            ShouldReadRequestBody = (req) => true;
        }
    }
}
