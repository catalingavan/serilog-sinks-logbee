using Microsoft.AspNetCore.Http;
using Serilog.Sinks.LogBee.RequestInfo;
using System.Text;

namespace Serilog.Sinks.LogBee.AspNetCore
{
    internal static class InternalHelpers
    {
        public static RequestProperties Create(HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            Uri requestUri = new Uri(GetDisplayUrl(request), UriKind.Absolute);

            var result = new RequestProperties(requestUri, request.Method)
            {
                Headers = ToDictionary(request.Headers),
                Cookies = ToDictionary(request.Cookies),
            };

            if (request.HasFormContentType)
                result.FormData = ToDictionary(request.Form);

            return result;
        }

        public static ResponseProperties Create(HttpResponse response)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            return new ResponseProperties(response.StatusCode)
            {
                Headers = ToDictionary(response.Headers)
            };
        }

        public static string? GetMachineName()
        {
            string? name = null;

            try
            {
                name =
                    Environment.GetEnvironmentVariable("CUMPUTERNAME") ??
                    Environment.GetEnvironmentVariable("HOSTNAME") ??
                    System.Net.Dns.GetHostName();
            }
            catch
            {
                // ignored
            }

            return name;
        }
        
        public static string GetDisplayUrl(HttpRequest request)
        {
            string value = request.Host.Value;
            string value2 = request.PathBase.Value;
            string value3 = request.Path.Value;
            string value4 = request.QueryString.Value;
            return new StringBuilder(request.Scheme.Length + "://".Length + value.Length + value2.Length + value3.Length + value4.Length).Append(request.Scheme).Append("://").Append(value)
                .Append(value2)
                .Append(value3)
                .Append(value4)
                .ToString();
        }

        public static Dictionary<string, string> ToDictionary(IRequestCookieCollection collection)
        {
            if (collection == null)
                return new Dictionary<string, string>();

            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (string key in collection.Keys)
            {
                if (string.IsNullOrWhiteSpace(key))
                    continue;
                
                string value = collection[key];
                result.TryAdd(key, value);
            }

            return result;
        }

        public static Dictionary<string, string> ToDictionary(IHeaderDictionary collection)
        {
            if (collection == null)
                return new Dictionary<string, string>();

            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (string key in collection.Keys)
            {
                if (string.IsNullOrWhiteSpace(key))
                    continue;

                string value = collection[key];
                result.TryAdd(key, value);
            }

            return result;
        }

        public static Dictionary<string, string> ToDictionary(IFormCollection collection)
        {
            if (collection == null)
                return new Dictionary<string, string>();

            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (string key in collection.Keys)
            {
                if (string.IsNullOrWhiteSpace(key))
                    continue;

                string value = collection[key];
                result.TryAdd(key, value);
            }

            return result;
        }

        public static Dictionary<string, string> ToDictionary(IQueryCollection collection)
        {
            if (collection == null)
                return new Dictionary<string, string>();

            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (string key in collection.Keys)
            {
                if (string.IsNullOrWhiteSpace(key))
                    continue;

                string value = collection[key];
                result.TryAdd(key, value);
            }

            return result;
        }
    }
}
