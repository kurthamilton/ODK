using Microsoft.AspNetCore.Http.Extensions;
using ODK.Services.Logging;
using HttpRequest = ODK.Services.Logging.HttpRequest;

namespace ODK.Web.Razor.Middleware
{
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
                    httpContext.Request.GetDisplayUrl(),
                    httpContext.Request.Method,
                    httpContext.User.Identity?.Name,
                    httpContext.Request.Headers.ToDictionary(x => x.Key, x => x.Value.ToString()),
                    httpContext.Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString()));
                await logger.LogError(ex, request);
            }
        }
    }
}
