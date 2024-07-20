using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using ODK.Core.Chapters;
using ODK.Core.Exceptions;
using ODK.Services.Caching;
using ODK.Services.Logging;
using ODK.Web.Common.Config.Settings;
using HttpRequest = ODK.Services.Logging.HttpRequest;

namespace ODK.Web.Razor.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    
    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context, 
        IRequestCache requestCache, 
        ILoggingService loggingService,
        AppSettings settings)
    {
        var statusCodeContext = new StatusCodeContext(context, new StatusCodePagesOptions(), _next);

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            if (!settings.Errors.Handle)
            {
                throw;
            }

            if (ex is OdkNotFoundException notFoundException)
            {                
                statusCodeContext.HttpContext.Response.StatusCode = 404;                
            }
            else
            {
                statusCodeContext.HttpContext.Response.StatusCode = 500;
            }

            await LogError(context, ex, loggingService);
            await HandleAsync(statusCodeContext.HttpContext, requestCache);
        }                
    }

    private async Task HandleAsync(HttpContext httpContext, IRequestCache requestCache)
    {
        var originalPath = httpContext.Request.Path;

        var chapter = await GetChapterFromPath(httpContext, requestCache);

        ResetHttpContext(httpContext);

        httpContext.Request.Path = chapter != null
            ? $"/{chapter.Name}/Error/{httpContext.Response.StatusCode}"
            : $"/Error/{httpContext.Response.StatusCode}";

        try
        {
            await _next(httpContext);
        }
        finally
        {
            httpContext.Request.Path = originalPath;
            httpContext.Features.Set<IStatusCodeReExecuteFeature?>(null);
        }
    }

    private async Task<Chapter?> GetChapterFromPath(HttpContext context, IRequestCache requestCache)
    {
        var originalPathParts = context.Request.Path.Value?.Split('/') ?? Array.Empty<string>();
        var chapterName = originalPathParts.Length > 1 ? originalPathParts[1] : null;

        try
        {
            return !string.IsNullOrEmpty(chapterName)
                ? await requestCache.GetChapterAsync(chapterName)
                : null;
        }        
        catch
        {
            return null;
        }
    }

    private async Task LogError(HttpContext httpContext, Exception ex, ILoggingService loggingService)
    {
        if (ex is OdkNotFoundException)
        {
            return;
        }

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
            await loggingService.LogError(ex, request);
        }
        catch
        {
            // do nothing
        }
    }

    private void ResetHttpContext(HttpContext context)
    {
        context.SetEndpoint(endpoint: null);

        var routeValuesFeature = context.Features.Get<IRouteValuesFeature>();
        routeValuesFeature?.RouteValues.Clear();
    }
}
