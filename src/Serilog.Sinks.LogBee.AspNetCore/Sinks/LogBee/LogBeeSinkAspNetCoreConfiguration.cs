using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Serilog.Sinks.LogBee.AspNetCore
{
    public class LogBeeSinkAspNetCoreConfiguration : LogBeeSinkConfiguration
    {
        public string[] ReadRequestBodyContentTypes { get; set; } = new[] { "application/javascript", "application/json", "application/xml", "text/plain", "text/xml", "text/html" };
        public string[] ReadResponseBodyContentTypes { get; set; } = new[] { "application/json" };
        
        public Func<HttpRequest, bool> ShouldReadRequestBody { get; set; } = (request) => true;
        public Func<HttpContext, bool> ShouldReadResponseBody { get; set; } = (context) => true;
        public Func<HttpRequest, KeyValuePair<string, StringValues>, bool> ShouldReadFormData { get; set; } = (request, formData) => true;
        public Func<HttpRequest, KeyValuePair<string, StringValues>, bool> ShouldReadRequestHeader { get; set; } = (request, headerData) => true;
        public Func<HttpRequest, KeyValuePair<string, string>, bool> ShouldReadRequestCookie { get; set; } = (request, headerData) => true;
        public Func<HttpContext, bool> ShouldLogRequest { get; set; } = (context) => true;
    }
}
