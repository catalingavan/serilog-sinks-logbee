using Microsoft.AspNetCore.Mvc;
using Serilog.Sinks.LogBee;
using Serilog.Sinks.LogBee.AspNetCore;
using System.Text.Json;

namespace Serilog.Sinks.LogBee_WebApp.Controllers
{
    public class HomeController : Controller
    {
        private LoggerContext? LoggerContext => HttpContext.GetLogBeeLoggerContext();

        private readonly ILogger<HomeController> _logger;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            _logger.LogInformation("Hello world from {Controller}", "Home");

            LoggerContext?.LogAsFile(JsonSerializer.Serialize(new { Prop = "Value" }), "File.json");

            return Content("HomeController Index() action");
        }
    }
}
