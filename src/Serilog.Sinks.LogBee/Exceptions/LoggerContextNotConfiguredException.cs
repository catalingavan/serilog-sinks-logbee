using System;

namespace Serilog.Sinks.LogBee.Exceptions
{
    internal class LoggerContextNotConfiguredException : Exception
    {
        public LoggerContextNotConfiguredException() : base(ErrorMessage()) { }

        private static string ErrorMessage()
        {
            return $"The loggerContext has not been yet configured. This error happens when you try to use the loggerContext before registering the Serilog LogBee Sink.";
        }
    }
}
