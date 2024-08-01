using System.Net;

namespace ODK.Web.Common.Extensions;

public static class HttpStatusCodeExtensions
{
    public static string ErrorPageTitle(this HttpStatusCode statusCode)
    {
        switch (statusCode)
        {
            case HttpStatusCode.Forbidden:
                return "Not permitted";
            case HttpStatusCode.NotFound:
                return "Page not found";
            default:
                return "An error has occurred";
        }
    }
}
