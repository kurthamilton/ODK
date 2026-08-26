namespace ODK.Web.Razor.Extensions;

public static class HttpRequestExtensions
{
    /// <summary>
    /// The request's Referer when it points back at this host, otherwise null - an off-site Referer would be
    /// an open-redirect vector, so it is never somewhere to send a member.
    /// </summary>
    public static string? LocalRefererOrDefault(this HttpRequest request)
    {
        var referer = request.Headers.Referer.ToString();

        return !string.IsNullOrEmpty(referer)
            && Uri.TryCreate(referer, UriKind.Absolute, out var refererUri)
            && string.Equals(refererUri.Host, request.Host.Host, StringComparison.OrdinalIgnoreCase)
                ? referer
                : null;
    }
}
