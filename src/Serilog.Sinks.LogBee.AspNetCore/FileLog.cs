namespace Serilog.Sinks.LogBee.AspNetCore
{
    internal class FileLog : IDisposable
    {
        public TemporaryFile TempFile { get; init; }
        public string FileName { get; init; }
        public long FileSize { get; init; }

        public FileLog(TemporaryFile tempFile, string fileName, long fileSize)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            if (fileSize < 0)
                throw new ArgumentOutOfRangeException(nameof(fileSize));

            TempFile = tempFile ?? throw new ArgumentNullException(nameof(tempFile));
            FileName = fileName;
            FileSize = fileSize;
        }

        public void Dispose()
        {
            TempFile.Dispose();
        }
    }
}
