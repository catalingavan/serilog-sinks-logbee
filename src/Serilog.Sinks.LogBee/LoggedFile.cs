using System;
using System.IO;

namespace Serilog.Sinks.LogBee;

internal class LoggedFile : IDisposable
{
    public string FilePath { get; private set; }
    public string FileName { get; private set; }
    public long FileSize { get; private set; }

    public LoggedFile(string filePath, string fileName, long fileSize)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath));

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentNullException(nameof(fileName));

        FilePath = filePath;
        FileName = fileName;
        FileSize = Math.Max(0, fileSize);
    }

    public void Dispose()
    {
        try
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }
        catch
        {
            // ignored
        }
    }
}
