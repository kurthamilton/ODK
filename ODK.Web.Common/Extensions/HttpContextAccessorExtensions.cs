using System;
using Microsoft.AspNetCore.Http;

namespace ODK.Web.Common.Extensions;

public static class HttpContextAccessorExtensions
{
    public static bool ForPath(this IHttpContextAccessor contextAccessor, string match, bool exactMatch = false)
    {
        var requestPath = contextAccessor.HttpContext?.Request.Path;
        if (requestPath == null)
        {
            return false;
        }
        
        return exactMatch 
            ? string.Equals(requestPath.Value, match, StringComparison.OrdinalIgnoreCase)
            : requestPath.Value.StartsWithSegments(match, StringComparison.InvariantCultureIgnoreCase);
    }
}
