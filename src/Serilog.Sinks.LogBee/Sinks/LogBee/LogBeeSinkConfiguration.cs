namespace Serilog.Sinks.LogBee
{
    public class LogBeeSinkConfiguration
    {
        public long MaximumAllowedFileSizeInBytes { get; set; } = 5 * 1024 * 1024;
    }
}
