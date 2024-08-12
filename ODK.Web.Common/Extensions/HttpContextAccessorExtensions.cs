using System;
using Microsoft.AspNetCore.Http;

namespace ODK.Web.Common.Extensions;

public static class HttpContextAccessorExtensions
{
    public static bool ForPath(this IHttpContextAccessor contextAccessor, string match)
    {
        var requestPath = contextAccessor.HttpContext?.Request.Path;
        return requestPath != null && string.Equals(requestPath, match, StringComparison.InvariantCultureIgnoreCase);
    }
}
