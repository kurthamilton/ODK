using System.Linq;
using Microsoft.AspNetCore.Http;

namespace ODK.Web.Common.Extensions;

public static class HttpRequestExtensions
{
    public static string? GetHeader(this HttpRequest request, string name)
        => request.Headers
            .GetCommaSeparatedValues(name)
            .FirstOrDefault();
}