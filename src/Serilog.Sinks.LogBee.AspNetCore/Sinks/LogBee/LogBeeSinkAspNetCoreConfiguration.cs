﻿using Microsoft.AspNetCore.Http;

namespace Serilog.Sinks.LogBee.AspNetCore
{
    public class LogBeeSinkAspNetCoreConfiguration
    {
        public string[] ReadRequestBodyContentTypes { get; set; } = new[] { "application/javascript", "application/json", "application/xml", "text/plain", "text/xml", "text/html" };
        public Func<HttpRequest, bool> ShouldReadRequestBody { get; set; } = (req) => true;
        public Func<HttpContext, bool> ShouldLogRequest { get; set; } = (req) => true;
    }
}
