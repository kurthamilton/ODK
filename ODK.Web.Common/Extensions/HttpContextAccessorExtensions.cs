using System;
using Microsoft.AspNetCore.Http;

namespace ODK.Web.Common.Extensions;

public static class HttpContextAccessorExtensions
{
    public static bool ForPath(this HttpContext context, string? match, bool exactMatch = false)
    {
        var requestPath = context?.Request.Path;
        if (requestPath == null)
        {
            return false;
        }

        return exactMatch
            ? string.Equals(requestPath.Value, match, StringComparison.OrdinalIgnoreCase)
            : requestPath.Value.StartsWithSegments(match, StringComparison.InvariantCultureIgnoreCase);
    }

    public static bool ForPath(this IHttpContextAccessor contextAccessor, string? match, bool exactMatch = false)
        => contextAccessor.HttpContext?.ForPath(match, exactMatch) == true;
}
