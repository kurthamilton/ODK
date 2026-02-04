using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using ODK.Core.Chapters;
using ODK.Core.Exceptions;
using ODK.Core.Platforms;
using ODK.Data.Core;
using ODK.Services;
using ODK.Services.Exceptions;
using ODK.Services.Logging;
using ODK.Web.Common.Extensions;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;
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
        IRequestStore requestStore,
        IUnitOfWork unitOfWork,
        IOdkRoutes odkRoutes,
        IPlatformProvider platformProvider)
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
            await LogError(context, ex, loggingService);

            if (context.Response.HasStarted)
            {
                // At this point you can't change the response to an error page
                throw;
            }

            context.Response.StatusCode = ex switch
            {
                OdkNotAuthenticatedException => 401,
                OdkNotAuthorizedException => 403,
                OdkNotFoundException => 404,
                _ => 500
            };

            await HandleAsync(context, requestStore, unitOfWork, odkRoutes, platformProvider);
        }
    }

    private async Task<Chapter?> FindChapter(
        HttpContext httpContext, IRequestStore requestStore, IUnitOfWork unitOfWork, IPlatformProvider platformProvider)
    {
        // We can't always get the chapter from the request store for 404s, as it matches by route params.
        // If we have a 404 as a result of not matching a route, we won't have any route params.

        var chapter = requestStore.ChapterOrDefault;
        if (chapter != null)
        {
            return chapter;
        }

        if (!requestStore.Loaded)
        {
            var requestContext = HttpRequestContext.Create(httpContext.Request);
            await requestStore.Load(new ServiceRequest
            {
                CurrentMemberIdOrDefault = httpContext.User.MemberIdOrDefault(),
                HttpRequestContext = requestContext,
                Platform = platformProvider.GetPlatform(requestContext.RequestUrl)
            });
        }

        // We might end up on a valid chapter route as a result of being redirected to a chapter error page,
        // so reset the request store just in case any downstream calls want to use the request store to get
        // the chapter based on the new route.
        var platform = requestStore.Platform;
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

        // DrunkenKnitwits chapter routes are like /{chapter.ShortName}/...
        if (platform == PlatformType.DrunkenKnitwits)
        {
            var fullName = Chapter.GetFullName(platform, pathParts[0]);
            return await unitOfWork.ChapterRepository.GetByName(platform, fullName).Run();
        }

        // Default routes are like
        // /groups/{chapter.Slug}/...
        // /my/groups/{chapter.Id}/...
        if (path.StartsWith("/groups/", StringComparison.OrdinalIgnoreCase) &&
            pathParts.Length > 1)
        {
            var slug = pathParts[1];
            return await unitOfWork.ChapterRepository.GetBySlug(platform, slug).Run();
        }
        else if (path.StartsWith("/my/groups/", StringComparison.OrdinalIgnoreCase) &&
            pathParts.Length > 2 &&
            Guid.TryParse(pathParts[2], out var chapterId))
        {
            return await unitOfWork.ChapterRepository.GetByIdOrDefault(platform, chapterId).Run();
        }

        return null;
    }

    private async Task HandleAsync(
        HttpContext httpContext,
        IRequestStore requestStore,
        IUnitOfWork unitOfWork,
        IOdkRoutes odkRoutes,
        IPlatformProvider platformProvider)
    {
        var request = httpContext.Request;
        var response = httpContext.Response;

        var originalMethod = request.Method;
        var originalPath = request.Path;

        var path = await GetErrorPath(httpContext, requestStore, unitOfWork, odkRoutes, platformProvider);

        ResetHttpContext(httpContext, path);

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
        HttpContext httpContext,
        IRequestStore requestStore,
        IUnitOfWork unitOfWork,
        IOdkRoutes odkRoutes,
        IPlatformProvider platformProvider)
    {
        var statusCode = httpContext.Response.StatusCode;

        var chapter = await FindChapter(httpContext, requestStore, unitOfWork, platformProvider);
        return odkRoutes.Error(chapter, statusCode);
    }

    private async Task LogError(
        HttpContext httpContext,
        Exception ex,
        ILoggingService loggingService)
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

    private void ResetHttpContext(HttpContext context, string? path)
    {
        context.SetEndpoint(endpoint: null);

        var routeValuesFeature = context.Features.Get<IRouteValuesFeature>();
        routeValuesFeature?.RouteValues.Clear();

        context.Response.Clear();
        context.Request.Method = HttpMethod.Get.Method;
        context.Request.Path = path;
    }
}