using System;

namespace Serilog.Sinks.LogBee
{
    public class LogBeeSinkConfiguration
    {
        public TimeSpan ClientTimeout { get; set; } = TimeSpan.FromSeconds(10);
        public long MaximumAllowedFileSizeInBytes { get; set; } = 5 * 1024 * 1024;
        public Func<Exception, string?> AppendExceptionDetails { get; set; } = (ex) => null;
    }
}
