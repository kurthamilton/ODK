using ODK.Services.Logging;

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
                await logger.LogError(ex, ex.Message);
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            
        }
    }
}
