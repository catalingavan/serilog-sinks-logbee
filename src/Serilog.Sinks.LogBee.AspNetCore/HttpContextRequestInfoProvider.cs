using Microsoft.AspNetCore.Http;
using Serilog.Sinks.LogBee.RequestInfo;

namespace Serilog.Sinks.LogBee.AspNetCore
{
    internal class HttpContextRequestInfoProvider : IRequestInfoProvider
    {
        private readonly HttpContext _httpContext;
        private readonly List<FileLog> _files;
        private readonly DateTime _startedAt;
        public HttpContextRequestInfoProvider(
            HttpContext httpContext)
        {
            _httpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            _files = new();
            _startedAt = DateTime.UtcNow;
        }

        public DateTime GetStartedAt() => _startedAt;
        public string? GetMachineName() => Serilog.Sinks.LogBee.InternalHelpers.GetMachineName();
        public RequestProperties GetRequestProperties()
        {
            var result = InternalHelpers.Create(_httpContext.Request);
            HttpContextLogger? httpContextLogger = InternalHelpers.GetHttpContextLogger(_httpContext);
            result.InputStream = httpContextLogger?.RequestBody;

            return result;
        }
        public ResponseProperties GetResponseProperties()
        {
            var result = InternalHelpers.Create(_httpContext.Response);
            HttpContextLogger? httpContextLogger = InternalHelpers.GetHttpContextLogger(_httpContext);
            if (httpContextLogger?.ResponseContentLength != null)
                result.ContentLength = httpContextLogger.ResponseContentLength.Value;

            return result;
        }
        public List<LoggedFile> GetFiles() => _files.Select(p => new LoggedFile(p.TempFile.FileName, p.FileName, p.FileSize)).ToList();

        public void LogAsFile(string contents, string? fileName = null)
        {
            FileLog? file = FileLog.Create(contents, fileName, 5 * 1024 * 1024);
            if (file != null)
                _files.Add(file);
        }

        public void Dispose()
        {
            foreach (var file in _files)
                file.Dispose();
        }
    }
}
