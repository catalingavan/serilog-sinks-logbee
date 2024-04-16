using System.Text.RegularExpressions;

namespace Serilog.Sinks.LogBee.Context
{
    public class LoggedFile : IDisposable
    {
        private static readonly Regex FILE_NAME_REGEX = new Regex(@"[^a-zA-Z0-9_\-\+\. ]+", RegexOptions.Compiled);

        public string FilePath { get; init; }
        public string FileName { get; init; }
        public long FileSize { get; init; }

        private LoggedFile(string filePath, string fileName, long fileSize)
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

        public static LoggedFile? Create(string contents, string? fileName = null)
        {
            if (string.IsNullOrEmpty(contents))
                return null;

            long fileSize = contents.Length;
            fileName = (string.IsNullOrWhiteSpace(fileName) ? "File" : fileName).Trim();
            fileName = FILE_NAME_REGEX.Replace(fileName, string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(fileName))
                fileName = "File";

            TemporaryFile? temporaryFile = null;
            try
            {
                temporaryFile = new TemporaryFile();
                File.WriteAllText(temporaryFile.FileName, contents);

                return new LoggedFile(temporaryFile.FileName, fileName, fileSize);
            }
            catch (Exception)
            {
                if (temporaryFile != null)
                    temporaryFile.Dispose();
            }

            return null;
        }
    }
}
