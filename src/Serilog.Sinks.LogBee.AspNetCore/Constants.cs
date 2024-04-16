using System.Text.RegularExpressions;

namespace Serilog.Sinks.LogBee.AspNetCore;

internal static class Constants
{
    public static readonly Regex FILE_NAME_REGEX = new Regex(@"[^a-zA-Z0-9_\-\+\. ]+", RegexOptions.Compiled);
    public const long MAXIMUM_ALLOWED_FILE_SIZE_IN_BYTES = 5 * 1024 * 1024;
}
