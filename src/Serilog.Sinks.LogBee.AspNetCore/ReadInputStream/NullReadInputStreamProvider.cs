using Microsoft.AspNetCore.Http;

namespace Serilog.Sinks.LogBee.AspNetCore.ReadInputStream
{
    public class NullReadInputStreamProvider : IReadInputStreamProvider
    {
        public string? ReadInputStream(HttpRequest request)
        {
            return null;
        }
    }
}
