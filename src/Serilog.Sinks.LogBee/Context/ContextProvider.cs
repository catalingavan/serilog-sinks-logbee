using System.Text.RegularExpressions;

namespace Serilog.Sinks.LogBee.Context
{
    public abstract class ContextProvider : IDisposable
    {
        private static readonly Regex FILE_NAME_REGEX = new Regex(@"[^a-zA-Z0-9_\-\+\. ]+", RegexOptions.Compiled);

        private readonly List<LoggedFile> _loggedFiles;
        public ContextProvider()
        {
            _loggedFiles = new();
        }

        public abstract DateTime GetStartedAt();
        public abstract RequestProperties GetRequestProperties();
        public abstract ResponseProperties GetResponseProperties();
        public abstract AuthenticatedUser? GetAuthenticatedUser();
        public virtual IntegrationClient GetIntegrationClient() => InternalHelpers.IntegrationClient.Value;
        public List<LoggedFile> GetLoggedFiles() => _loggedFiles.ToList();

        public void LogAsFile(string contents, string? fileName = null)
        {
            if (string.IsNullOrEmpty(contents))
                return;

            fileName = (string.IsNullOrWhiteSpace(fileName) ? "File" : fileName).Trim();
            fileName = FILE_NAME_REGEX.Replace(fileName, string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(fileName))
                fileName = "File";

            TemporaryFile? temporaryFile = null;
            try
            {
                temporaryFile = new TemporaryFile();
                File.WriteAllText(temporaryFile.FileName, contents);

                _loggedFiles.Add(new LoggedFile(temporaryFile.FileName, fileName, contents.Length));
            }
            catch (Exception)
            {
                if (temporaryFile != null)
                    temporaryFile.Dispose();
            }
        }

        public virtual void Dispose()
        {
            foreach (var file in _loggedFiles)
                file.Dispose();
        }
    }
}
