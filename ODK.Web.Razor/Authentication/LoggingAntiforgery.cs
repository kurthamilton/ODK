using Microsoft.AspNetCore.Antiforgery;
using Microsoft.Net.Http.Headers;
using ODK.Services.Logging;
using ODK.Web.Razor.Services;

namespace ODK.Web.Razor.Authentication;

/// <summary>
/// Decorates <see cref="IAntiforgery"/> to log antiforgery (CSRF) validation failures. Both MVC
/// controllers (<c>AutoValidateAntiforgeryTokenAttribute</c>) and Razor Page handlers validate through
/// <see cref="IAntiforgery.ValidateRequestAsync"/>; the antiforgery filter catches the resulting
/// <see cref="AntiforgeryValidationException"/> and turns it into a bare 400 before it reaches any
/// middleware, and the framework itself only logs it at Information - so a failed CSRF check is otherwise
/// invisible ("400 with nothing logged"). This logs the failure with request context at Error, then
/// rethrows so the 400 response is unchanged.
/// </summary>
public class LoggingAntiforgery : IAntiforgery
{
    private const string SecFetchSiteHeaderName = "Sec-Fetch-Site";

    private readonly IAntiforgery _inner;
    private readonly ILogger<LoggingAntiforgery> _logger;

    public LoggingAntiforgery(IAntiforgery inner, ILogger<LoggingAntiforgery> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public AntiforgeryTokenSet GetAndStoreTokens(HttpContext httpContext)
        => _inner.GetAndStoreTokens(httpContext);

    public AntiforgeryTokenSet GetTokens(HttpContext httpContext)
        => _inner.GetTokens(httpContext);

    public Task<bool> IsRequestValidAsync(HttpContext httpContext)
        => _inner.IsRequestValidAsync(httpContext);

    public void SetCookieTokenAndHeader(HttpContext httpContext)
        => _inner.SetCookieTokenAndHeader(httpContext);

    public async Task ValidateRequestAsync(HttpContext httpContext)
    {
        try
        {
            await _inner.ValidateRequestAsync(httpContext);
        }
        catch (AntiforgeryValidationException exception)
        {
            LogFailure(httpContext, exception);
            throw;
        }
    }

    private static string Display(string? value) => value ?? "(none)";

    private static string? GetHeaderOrDefault(IHeaderDictionary headers, string name)
    {
        var value = headers[name].ToString();
        return !string.IsNullOrEmpty(value) ? value : null;
    }

    private void LogFailure(HttpContext httpContext, AntiforgeryValidationException exception)
    {
        // A browser always sends Origin on a POST navigation and Sec-Fetch-Site on a same-site submit;
        // a scripted POST typically sends neither, whatever its user agent claims. Referer names the page
        // whose form failed. Together with the exception message (which distinguishes a missing request
        // token from a missing cookie) these separate a form rendered without a token from a bot posting
        // blind.
        var request = httpContext.Request;
        var origin = GetHeaderOrDefault(request.Headers, HeaderNames.Origin);
        var secFetchSite = GetHeaderOrDefault(request.Headers, SecFetchSiteHeaderName);

        // Neither header present means no browser sent this, so it is a bot posting blind rather than a
        // real failure worth an error. Both are required: Origin survives any Referrer-Policy, and
        // Sec-Fetch-Site is a forbidden header name that page script cannot forge - so a client missing
        // both is not a browser. Referer is deliberately not part of this test; privacy tooling and
        // corporate proxies strip it from genuine requests, and adding a Referrer-Policy header to the
        // app would remove it from every request. The framework still logs the failure at Information.
        if (origin == null && secFetchSite == null)
        {
            return;
        }

        // Resolved per-request: this decorator is a singleton, ILoggingService is scoped.
        var loggingService = httpContext.RequestServices.GetRequiredService<ILoggingService>();
        if (loggingService.IgnoreException(exception, HttpRequestContext.Create(request)))
        {
            return;
        }

        _logger.LogError(
            exception,
            "Antiforgery (CSRF) validation failed for {Method} {Path}. User={User}, Origin={Origin}, " +
            "SecFetchSite={SecFetchSite}, Referer={Referer}",
            request.Method,
            request.Path,
            httpContext.User.Identity?.Name ?? "(anonymous)",
            Display(origin),
            Display(secFetchSite),
            Display(GetHeaderOrDefault(request.Headers, HeaderNames.Referer)));
    }
}
