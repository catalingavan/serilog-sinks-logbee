using System.Text.RegularExpressions;

namespace Serilog.Sinks.LogBee
{
    internal class FileLog : IDisposable
    {
        private static readonly Regex FILE_NAME_REGEX = new Regex(@"[^a-zA-Z0-9_\-\+\. ]+", RegexOptions.Compiled);

        public TemporaryFile TempFile { get; init; }
        public string FileName { get; init; }
        public long FileSize { get; init; }

        private FileLog(TemporaryFile tempFile, string fileName, long fileSize)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            if (fileSize < 0)
                throw new ArgumentOutOfRangeException(nameof(fileSize));

            TempFile = tempFile ?? throw new ArgumentNullException(nameof(tempFile));
            FileName = fileName;
            FileSize = fileSize;
        }

        public static FileLog? Create(string contents, string? fileName, long maximumAllowedFileSizeInBytes)
        {
            if (string.IsNullOrEmpty(contents))
                return null;

            long fileSize = contents.Length;
            if (fileSize > maximumAllowedFileSizeInBytes)
                return null;

            fileName = (string.IsNullOrWhiteSpace(fileName) ? "File" : fileName).Trim();
            fileName = FILE_NAME_REGEX.Replace(fileName, string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(fileName))
                fileName = "File";

            TemporaryFile? temporaryFile = null;
            try
            {
                temporaryFile = new TemporaryFile();
                File.WriteAllText(temporaryFile.FileName, contents);

                return new FileLog(temporaryFile, fileName, fileSize);
            }
            catch (Exception)
            {
                if (temporaryFile != null)
                    temporaryFile.Dispose();
            }

            return null;
        }

        public void Dispose()
        {
            TempFile.Dispose();
        }
    }
}
