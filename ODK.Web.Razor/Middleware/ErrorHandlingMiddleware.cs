using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using ODK.Core.Chapters;
using ODK.Core.Exceptions;
using ODK.Core.Platforms;
using ODK.Data.Core;
using ODK.Services.Exceptions;
using ODK.Services.Logging;
using ODK.Web.Common.Extensions;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;
using ODK.Web.Razor.Services;
using OdkHttpRequest = ODK.Services.Logging.HttpRequest;

namespace ODK.Web.Razor.Middleware;

public class ErrorHandlingMiddleware
{
    // Request headers whose values carry credentials and must not be logged.
    private static readonly HashSet<string> SensitiveHeaders =
        new(StringComparer.OrdinalIgnoreCase) { "Cookie", "Authorization" };

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
        IOdkRoutes odkRoutes)
    {
        try
        {
            await _next(context);

            if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
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

            await Handle(context, requestStore, unitOfWork, odkRoutes);
        }
    }

    // Credential-bearing header values are replaced with "[redacted]"; everything else is kept as-is.
    internal static Dictionary<string, string> RedactSensitiveHeaders(IHeaderDictionary headers)
        => headers.ToDictionary(
            x => x.Key,
            x => SensitiveHeaders.Contains(x.Key) ? "[redacted]" : x.Value.ToString());

    private async Task<Chapter?> FindChapter(
        HttpContext httpContext, IRequestStore requestStore, IUnitOfWork unitOfWork)
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
            var currentMemberIdOrDefault = httpContext.User.MemberIdOrDefault();
            await requestStore.Load(requestContext, currentMemberIdOrDefault, httpContext.User.SignedInMemberIds());
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
        // /my/groups/{chapter.Slug}/...
        string? segment = null;
        if (path.StartsWith("/groups/", StringComparison.OrdinalIgnoreCase) && pathParts.Length > 1)
        {
            segment = pathParts[1];
        }
        else if (path.StartsWith("/my/groups/", StringComparison.OrdinalIgnoreCase) && pathParts.Length > 2)
        {
            segment = pathParts[2];
        }

        if (segment == null)
        {
            return null;
        }

        // A group's id stands in the same segment as its slug on the image and subscription-alert
        // endpoints, and on a group admin URL that names the id, so either shape reaches here.
        return Guid.TryParse(segment, out var chapterId)
            ? await unitOfWork.ChapterRepository.GetByIdOrDefault(platform, chapterId).Run()
            : await unitOfWork.ChapterRepository.GetBySlug(platform, segment).Run();
    }

    private async Task Handle(
        HttpContext httpContext,
        IRequestStore requestStore,
        IUnitOfWork unitOfWork,
        IOdkRoutes odkRoutes)
    {
        var request = httpContext.Request;
        var response = httpContext.Response;

        var originalMethod = request.Method;
        var originalPath = request.Path;

        /* Captured before the reset and re-applied after it, because Response.Clear() returns the status
           code to 200 along with the headers and body. The error page is re-executed rather than
           redirected to, so this response is still the failing URL's and has to carry its status. */
        var statusCode = response.StatusCode;

        var path = await GetErrorPath(httpContext, requestStore, unitOfWork, odkRoutes, statusCode);

        ResetHttpContext(httpContext, path);

        response.StatusCode = statusCode;

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

    private async Task<string> GetErrorPath(
        HttpContext httpContext,
        IRequestStore requestStore,
        IUnitOfWork unitOfWork,
        IOdkRoutes odkRoutes,
        int statusCode)
    {
        var chapter = await FindChapter(httpContext, requestStore, unitOfWork);
        return odkRoutes.Error(chapter, statusCode);
    }

    private async Task LogError(
        HttpContext httpContext,
        Exception ex,
        ILoggingService loggingService)
    {
        // A request whose client has gone is not a failure of the app, and there is nobody left to serve
        // either way.
        if (httpContext.ClientDisconnected(ex))
        {
            return;
        }

        var requestContext = HttpRequestContext.Create(httpContext.Request);

        if (loggingService.IgnoreException(ex, requestContext))
        {
            return;
        }

        // Redact credential-bearing headers so auth cookies/tokens don't end up in logs.
        var headers = RedactSensitiveHeaders(httpContext.Request.Headers);

        var form = new Dictionary<string, string>();

        try
        {
            // Record which fields were submitted, but not their values - form bodies routinely contain
            // passwords and personal data.
            form = httpContext.Request.Form.ToDictionary(x => x.Key, _ => "[redacted]");
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
            else if (ex is OdkNotAuthenticatedException)
            {
                // The caller reached something they are not signed in for, and the app answered with the
                // 401 page it has for exactly that. A member whose session ended with a tab open raises
                // this routinely, so it is a warning and not something to be notified about.
                await loggingService.Warn(
                    $"{ex.GetType().Name}: {httpContext.Request.Method} {httpContext.Request.Path}",
                    new Dictionary<string, string?>
                    {
                        ["Member"] = httpContext.User.MemberIdOrDefault()?.ToString()
                    });
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

    private void ResetHttpContext(HttpContext context, string path)
    {
        context.SetEndpoint(endpoint: null);

        context.Features.Get<IRouteValuesFeature>()?.RouteValues.Clear();

        // Only clear if it hasn't started (Clear throws if started)
        if (!context.Response.HasStarted)
        {
            context.Response.Clear();
        }

        context.Request.Method = HttpMethods.Get;
        context.Request.Path = path;
    }
}