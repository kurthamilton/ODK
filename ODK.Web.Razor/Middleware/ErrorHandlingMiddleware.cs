using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using ODK.Core.Exceptions;
using ODK.Data.Core;
using ODK.Services.Caching;
using ODK.Services.Exceptions;
using ODK.Services.Logging;
using ODK.Web.Common.Config.Settings;
using ODK.Web.Razor.Services;
using System.Text.RegularExpressions;
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
        IUnitOfWork unitOfWork,
        AppSettings appSettings,
        IRequestStore requestStore)
    {
        try
        {
            await _next(context);

            if (context.Response.StatusCode == 404)
            {                
                throw new OdkNotFoundException($"Path not found: {context.Request.Path}");
            }
        }
        catch (Exception ex)
        {            
            await LogError(context, ex, loggingService, appSettings);

            context.Response.StatusCode = ex switch
            {
                OdkNotAuthenticatedException => 401,
                OdkNotAuthorizedException => 403,
                OdkNotFoundException => 404,
                _ => 500
            };

            await HandleAsync(context, requestCache, unitOfWork, requestStore);            
        }                
    }

    private async Task HandleAsync(
        HttpContext httpContext, 
        IRequestCache requestCache, 
        IUnitOfWork unitOfWork, 
        IRequestStore requestStore)
    {
        var request = httpContext.Request;
        var response = httpContext.Response;

        var originalMethod = request.Method;
        var originalPath = request.Path;

        var path = await GetErrorPath(httpContext, requestCache, unitOfWork, requestStore);

        ResetHttpContext(httpContext);

        request.Method = HttpMethod.Get.Method;
        request.Path = path;

        try
        {
            await _next(httpContext);
        }
        finally
        {
            request.Method = originalMethod;
            request.Path = originalPath;
            httpContext.Features.Set<IStatusCodeReExecuteFeature?>(null);
        }
    }

    private async Task<string?> GetErrorPath(
        HttpContext context, IRequestCache requestCache, IUnitOfWork unitOfWork, IRequestStore requestStore)
    {
        var statusCode = context.Response.StatusCode;
        var defaultPath = $"/error/{statusCode}";

        var originalPathParts = context.Request.Path.Value?.Split('/') ?? [];
        if (originalPathParts.Length <= 1)
        {
            return defaultPath;
        }

        if (string.Equals(originalPathParts[1], "groups", StringComparison.InvariantCultureIgnoreCase) && 
            originalPathParts.Length > 2)
        {
            var slug = originalPathParts[2];
            var chapter = await unitOfWork.ChapterRepository.GetBySlug(slug).Run();

            if (chapter != null)
            {
                return $"/groups/{slug}/error/{context.Response.StatusCode}";
            }            
        }
        else
        {
            var chapterName = originalPathParts[1];
            try
            {
                var chapter = await requestCache.GetChapterAsync(requestStore.Platform, chapterName);
                if (chapter != null)
                {
                    return $"/{chapter.ShortName}/error/{statusCode}";
                }
            }
            catch
            {
                return defaultPath;
            }            
        }        

        return defaultPath;
    }

    private async Task LogError(HttpContext httpContext, Exception ex, ILoggingService loggingService, AppSettings appSettings)
    {
        if (ex is OdkNotFoundException &&
            appSettings.Logging.NotFound.IgnorePatterns.Any(x => Regex.IsMatch(httpContext.Request.Path, x)))
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
            await loggingService.Error(ex, request);
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
