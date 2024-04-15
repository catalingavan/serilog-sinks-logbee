using Microsoft.AspNetCore.Http;

namespace Serilog.Sinks.LogBee.AspNetCore.ReadInputStream
{
    internal interface IReadInputStreamProvider
    {
        string? ReadInputStream(HttpRequest request);
    }
}
