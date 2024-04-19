using System;

namespace Serilog.Sinks.LogBee;

public class NonWebLoggerContext : LoggerContext2
{
    public NonWebLoggerContext(
            string url = "http://application",
            string httpMethod = "GET")
    {
        if (string.IsNullOrWhiteSpace(httpMethod))
            throw new ArgumentNullException(nameof(httpMethod));

        Uri? absoluteUri = null;
        if (!string.IsNullOrWhiteSpace(url) && Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
            absoluteUri = uri.IsAbsoluteUri ? uri : new Uri(new Uri("http://application"), uri);

        if (absoluteUri == null)
            absoluteUri = new Uri("http://application", UriKind.Absolute);
    }
}
