using ODK.Services.Authentication;
using Serilog;
using Serilog.Context;

namespace ODK.Web.Razor.Middleware;

/// <summary>
/// Middleware for adding data from the HTTP context to LogContext,
/// which will subequently be added to LogEvents via .Enrich.FromLogContext()
/// </summary>
public class HttpContextLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDiagnosticContext _diagnosticContext;

    public HttpContextLoggingMiddleware(
        RequestDelegate next,
        IDiagnosticContext diagnosticContext)

    // diagnosticContext from SeriiLog Middleware so that properties are attached to request summary messages
    {
        _next = next;
        _diagnosticContext = diagnosticContext;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        var claimsUser = new OdkClaimsUser(httpContext.User.Claims);
        
        // If properties are null they won't be added to the Context and subsequent LogActions
        using var memberIdProp = LogContext.PushProperty("MemberId", claimsUser.MemberId);
        
        // For Serilog Request summary messages
        _diagnosticContext.Set("MemberId", claimsUser.MemberId);

        await _next(httpContext);
    }
}
