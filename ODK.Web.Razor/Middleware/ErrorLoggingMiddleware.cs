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
            var headers = httpContext.Request.Headers
                .ToDictionary(x => x.Key, x => x.Value.ToString());

            var form = new Dictionary<string, string>();
            try
            {
                form = httpContext.Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());
            }
            catch
            {
                // do nothing
            }

            var request = new HttpRequest(
                url: httpContext.Request.GetDisplayUrl(),
                method: httpContext.Request.Method,
                username: httpContext.User.Identity?.Name,
                headers: headers,
                form: form
            );

            try
            {
                await logger.LogError(ex, request);
            }            
            catch (Exception inner)
            {
                int stop = 1;
            }
        }
    }
}
