using ODK.Core.Platforms;
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
        IRequestStore requestStore)
    {
        var endpoint = context.GetEndpoint();

        if (endpoint?.Metadata.GetMetadata<SkipRequestStoreMiddlewareAttribute>() is not null)
        {
            await _next(context);
            return;
        }

        var requestContext = HttpRequestContext.Create(context.Request);
        var currentMemberIdOrDefault = context.User.MemberIdOrDefault();
        await requestStore.Load(requestContext, currentMemberIdOrDefault);

        await _next(context);
    }
}