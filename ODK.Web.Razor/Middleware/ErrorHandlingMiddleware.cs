using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using ODK.Core.Exceptions;
using ODK.Services.Exceptions;
using ODK.Services.Logging;
using ODK.Web.Common.Config.Settings;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Services;
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
        ILoggingService loggingService,
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

            await HandleAsync(context, requestStore);
        }
    }

    private async Task HandleAsync(
        HttpContext httpContext,
        IRequestStore requestStore)
    {
        var request = httpContext.Request;
        var response = httpContext.Response;

        var originalMethod = request.Method;
        var originalPath = request.Path;

        var path = await GetErrorPath(httpContext, requestStore);

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
        HttpContext context, IRequestStore requestStore)
    {
        var statusCode = context.Response.StatusCode;
        var chapter = await requestStore.GetChapterOrDefault();
        var platform = requestStore.Platform;

        return OdkRoutes.Error(requestStore.Platform, chapter, statusCode);
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
            if (ex is OdkNotFoundException)
            {
                await loggingService.Warn(ex.Message);
            }
            else
            {
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