using Microsoft.AspNetCore.Http.Extensions;
using ODK.Services.Logging;
using HttpRequest = ODK.Services.Logging.HttpRequest;

namespace ODK.Web.Razor.Middleware;

public class ErrorLoggingMiddleware
{
    private readonly RequestDelegate _next;
    
    public ErrorLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, ILoggingService logger)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            HttpRequest request = new HttpRequest(
                url: httpContext.Request.GetDisplayUrl(),
                method: httpContext.Request.Method,
                username: httpContext.User.Identity?.Name,
                headers: httpContext.Request.Headers.ToDictionary(x => x.Key, x => x.Value.ToString()),
                form: httpContext.Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString()));
            await logger.LogError(ex, request);
        }
    }
}
