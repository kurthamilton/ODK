using Microsoft.AspNetCore.Antiforgery;
using ODK.Services.Logging;
using ODK.Web.Razor.Services;

namespace ODK.Web.Razor.Authentication;

/// <summary>
/// Decorates <see cref="IAntiforgery"/> to log antiforgery (CSRF) validation failures. Both MVC
/// controllers (<c>AutoValidateAntiforgeryTokenAttribute</c>) and Razor Page handlers validate through
/// <see cref="IAntiforgery.ValidateRequestAsync"/>; the antiforgery filter catches the resulting
/// <see cref="AntiforgeryValidationException"/> and turns it into a bare 400 before it reaches any
/// middleware, and the framework itself only logs it at Information - so a failed CSRF check is otherwise
/// invisible ("400 with nothing logged"). This logs the failure with request context at Warning, then
/// rethrows so the 400 response is unchanged.
/// </summary>
public class LoggingAntiforgery : IAntiforgery
{
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
            // Resolved per-request: this decorator is a singleton, ILoggingService is scoped.
            var loggingService = httpContext.RequestServices.GetRequiredService<ILoggingService>();
            if (!loggingService.IgnoreException(exception, HttpRequestContext.Create(httpContext.Request)))
            {
                var request = httpContext.Request;
                _logger.LogError(
                    exception,
                    "Antiforgery (CSRF) validation failed for {Method} {Path}. User={User}",
                    request.Method,
                    request.Path,
                    httpContext.User.Identity?.Name ?? "(anonymous)");
            }

            throw;
        }
    }
}
