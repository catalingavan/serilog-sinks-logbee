using System.Text.RegularExpressions;

namespace Serilog.Sinks.LogBee.Context
{
    public class LoggedFile : IDisposable
    {
        public string FilePath { get; init; }
        public string FileName { get; init; }
        public long FileSize { get; init; }

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
}
