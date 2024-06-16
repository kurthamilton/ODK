using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using ODK.Core.Chapters;
using ODK.Services.Caching;

namespace ODK.Web.Razor.Middleware;

public class ErrorPageMiddleware
{
    private readonly RequestDelegate _next;
    
    public ErrorPageMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IRequestCache requestCache)
    {
        await _next(context);
        
        // Do nothing if a response body has already been provided.
        if (context.Response.HasStarted
            || context.Response.StatusCode < 400
            || context.Response.StatusCode >= 600
            || context.Response.ContentLength.HasValue
            || !string.IsNullOrEmpty(context.Response.ContentType))
        {
            return;
        }

        var statusCodeContext = new StatusCodeContext(context, new StatusCodePagesOptions(), _next);
        await HandleAsync(statusCodeContext, requestCache);
    }

    private async Task HandleAsync(StatusCodeContext context, IRequestCache requestCache)
    {
        HttpContext httpContext = context.HttpContext;

        PathString originalPath = httpContext.Request.Path;

        Chapter? chapter = await GetChapterFromPath(httpContext, requestCache);

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
        string[] originalPathParts = context.Request.Path.Value?.Split('/') ?? Array.Empty<string>();
        string? chapterName = originalPathParts.Length > 1 ? originalPathParts[1] : null;

        return !string.IsNullOrEmpty(chapterName)
            ? await requestCache.GetChapterAsync(chapterName)
            : null;
    }

    private void ResetHttpContext(HttpContext context)
    {
        context.SetEndpoint(endpoint: null);

        var routeValuesFeature = context.Features.Get<IRouteValuesFeature>();
        routeValuesFeature?.RouteValues.Clear();
    }
}
