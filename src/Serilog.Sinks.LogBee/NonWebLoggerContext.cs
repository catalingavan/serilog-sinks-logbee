using Serilog.Sinks.LogBee.ContextProperties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Serilog.Sinks.LogBee;

public class NonWebLoggerContext : LoggerContext
{
    private const string    DEFAULT_URI = "http://application";
    private const string    DEFAULT_HTTP_METHOD = "GET";
    private const int       DEFAULT_STATUS_CODE = 200;

    private RequestProperties _requestProperties;
    private ResponseProperties _responseProperties;
    private AuthenticatedUser? _authenticatedUser;
    private List<string> _keywords;

    public NonWebLoggerContext(string url = DEFAULT_URI, string httpMethod = DEFAULT_HTTP_METHOD)
    {
        _requestProperties = new RequestProperties(new Uri(DEFAULT_URI), DEFAULT_HTTP_METHOD);
        _responseProperties = new ResponseProperties(DEFAULT_STATUS_CODE);
        _keywords = new();

        Reset(url, httpMethod);
    }

    internal override RequestProperties GetRequestProperties() => _requestProperties;
    internal override ResponseProperties GetResponseProperties() => _responseProperties;
    internal override AuthenticatedUser? GetAuthenticatedUser() => _authenticatedUser;
    internal override List<string> GetKeywords() => _keywords.ToList();

    /// <summary>
    /// Resets the loggerContext (captured logs, exceptions, loggedFiles).
    /// </summary>
    public void Reset(string url = DEFAULT_URI, string httpMethod = DEFAULT_HTTP_METHOD)
    {
        if (string.IsNullOrWhiteSpace(httpMethod))
            throw new ArgumentNullException(nameof(httpMethod));

        Uri? absoluteUri = null;
        if (!string.IsNullOrWhiteSpace(url) && Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
            absoluteUri = uri.IsAbsoluteUri ? uri : new Uri(new Uri("http://application"), uri);

        if (absoluteUri == null)
            absoluteUri = new Uri("http://application", UriKind.Absolute);

        _requestProperties = new RequestProperties(absoluteUri, httpMethod);
        _responseProperties = new ResponseProperties(200);
        _keywords = new();

        base.InternalReset();
    }

    /// <summary>
    /// Sets the Request properties of the loggerContext (implicitly what will be sent to logbee.net). Using this method also resets the loggerContext.
    /// </summary>
    public void SetRequestProperties(RequestProperties properties)
    {
        _requestProperties = properties ?? throw new ArgumentNullException(nameof(properties));
    }

    /// <summary>
    /// Sets the Response properties of the loggerContext (implicitly what will be sent to logbee.net).
    /// </summary>
    public void SetResponseProperties(ResponseProperties properties)
    {
        _responseProperties = properties ?? throw new ArgumentNullException(nameof(properties));
    }

    /// <summary>
    /// Sets the authenticated user
    /// </summary>
    public void SetAuthenticatedUser(AuthenticatedUser user)
    {
        _authenticatedUser = user ?? throw new ArgumentNullException(nameof(user));
    }

    /// <summary>
    /// Sets the keywords associated for the request saved to logbee.net
    /// </summary>
    public void SetKeywords(List<string> keywords)
    {
        _keywords = keywords ?? throw new ArgumentNullException(nameof(keywords));
    }
}
