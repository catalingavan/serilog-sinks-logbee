using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Serilog.Sinks.LogBee.Context
{
    public abstract class ContextProvider : IDisposable
    {
        private static readonly Regex FILE_NAME_REGEX = new Regex(@"[^a-zA-Z0-9_\-\+\. ]+", RegexOptions.Compiled);

        private readonly List<LoggedFile> _loggedFiles;
        private List<string> _keywords;
        public ContextProvider()
        {
            _loggedFiles = new();
            _keywords = new();
        }

        public abstract DateTime GetStartedAt();
        public abstract RequestProperties GetRequestProperties();
        public abstract ResponseProperties GetResponseProperties();
        public abstract AuthenticatedUser? GetAuthenticatedUser();
        public virtual IntegrationClient GetIntegrationClient() => InternalHelpers.IntegrationClient.Value;
        public List<LoggedFile> GetLoggedFiles() => _loggedFiles.ToList();
        public List<string> GetKeywords() => _keywords.ToList();

        public void LogAsFile(string contents, string? fileName = null)
        {
            if (string.IsNullOrEmpty(contents))
                return;

            fileName = (fileName == null || string.IsNullOrWhiteSpace(fileName) ? "File" : fileName).Trim();
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

        public void SetKeywords(List<string> keywords)
        {
            _keywords = keywords ?? throw new ArgumentNullException(nameof(keywords));
        }

        public virtual void Dispose()
        {
            foreach (var file in _loggedFiles)
                file.Dispose();
        }
    }
}
