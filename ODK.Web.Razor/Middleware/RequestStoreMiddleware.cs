using ODK.Core.Platforms;
using ODK.Services;
using ODK.Web.Common.Extensions;
using ODK.Web.Common.Services;
using ODK.Web.Razor.Attributes;

namespace ODK.Web.Razor.Middleware;

public class RequestStoreMiddleware
{
    private readonly RequestDelegate _next;

    public RequestStoreMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IRequestStore requestStore,
        IRequestStoreFactory requestStoreFactory,
        IPlatformProvider platformProvider)
    {
        var endpoint = context.GetEndpoint();

        if (endpoint?.Metadata.GetMetadata<SkipRequestStoreMiddlewareAttribute>() is not null)
        {
            await _next(context);
            return;
        }

        var httpRequestContext = HttpRequestContext.Create(context.Request);

        var serviceRequest = new ServiceRequest
        {
            CurrentMemberIdOrDefault = context.User.MemberIdOrDefault(),
            HttpRequestContext = httpRequestContext,
            Platform = platformProvider.GetPlatform(httpRequestContext.BaseUrl)
        };

        await requestStore.Load(serviceRequest);
        await _next(context);
    }
}
