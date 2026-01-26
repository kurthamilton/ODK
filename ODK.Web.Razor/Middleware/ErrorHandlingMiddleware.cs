using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using ODK.Core.Chapters;
using ODK.Core.Exceptions;
using ODK.Core.Platforms;
using ODK.Data.Core;
using ODK.Services.Exceptions;
using ODK.Services.Logging;
using ODK.Web.Common.Config.Settings;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;
using ODK.Web.Razor.Services;
using OdkHttpRequest = ODK.Services.Logging.HttpRequest;

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
        ILoggingService loggingService,
        AppSettings appSettings,
        IRequestStore requestStore,
        IUnitOfWork unitOfWork)
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

            await HandleAsync(context, ex, requestStore, unitOfWork);
        }
    }

    private async Task<Chapter?> FindChapter(
        HttpContext httpContext, IRequestStore requestStore, IUnitOfWork unitOfWork)
    {
        // We can't always get the chapter from the request store for 404s, as it matches by route params.
        // If we have a 404 as a result of not matching a route, we won't have any route params.

        var chapter = await requestStore.GetChapterOrDefault();
        if (chapter != null)
        {
            return chapter;
        }

        // We might end up on a valid chapter route as a result of being redirected to a chapter error page,
        // so reset the request store just in case any downstream calls want to use the request store to get
        // the chapter based on the new route.
        requestStore.Reset();

        var request = httpContext.Request;
        var path = request.Path.Value;
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        var pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (pathParts.Length == 0)
        {
            return null;
        }

        // Default routes are like /{chapter.Slug}/...
        // DrunkenKnitwits chapter routes are like /{chapter.ShortName}/...
        var platform = requestStore.Platform;
        if (platform == PlatformType.DrunkenKnitwits)
        {
            var fullName = Chapter.GetFullName(platform, pathParts[0]);
            return await unitOfWork.ChapterRepository.GetByName(fullName).Run();
        }

        var slug = pathParts[0];
        return await unitOfWork.ChapterRepository.GetBySlug(slug).Run();
    }

    private async Task HandleAsync(
        HttpContext httpContext,
        Exception ex,
        IRequestStore requestStore,
        IUnitOfWork unitOfWork)
    {
        var request = httpContext.Request;
        var response = httpContext.Response;

        var originalMethod = request.Method;
        var originalPath = request.Path;

        var path = await GetErrorPath(httpContext, ex, requestStore, unitOfWork);

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
        HttpContext httpContext, Exception ex, IRequestStore requestStore, IUnitOfWork unitOfWork)
    {
        var statusCode = httpContext.Response.StatusCode;
        var platform = requestStore.Platform;

        var chapter = await FindChapter(httpContext, requestStore, unitOfWork);
        return OdkRoutes.Error(platform, chapter, statusCode);
    }

    private async Task LogError(
        HttpContext httpContext,
        Exception ex,
        ILoggingService loggingService,
        AppSettings appSettings)
    {
        if (ex is OdkNotFoundException)
        {
            if (loggingService.IgnoreUnknownRequestPath(HttpRequestContext.Create(httpContext.Request)))
            {
                return;
            }
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

        try
        {
            if (ex is OdkNotFoundException)
            {
                await loggingService.Warn(ex.Message);
            }
            else
            {
                var request = new OdkHttpRequest(
                    url: httpContext.Request.GetDisplayUrl(),
                    method: httpContext.Request.Method,
                    username: httpContext.User.Identity?.Name,
                    headers: headers,
                    form: form
                );

                await loggingService.Error(ex, request);
            }
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