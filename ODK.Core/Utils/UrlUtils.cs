using ODK.Core.Web;

namespace ODK.Core.Utils;

public static class UrlUtils
{
    public static string BaseUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return string.Empty;
        }

        var uri = new Uri(url);
        return uri.GetLeftPart(UriPartial.Authority);
    }

    public static string NormalisePath(string path)
    {
        path = path.EnsureLeading("/");

        var parts = path.Split('/');

        return Path.HasExtension(parts.Last())
            ? path
            : path.EnsureTrailing("/");
    }

    public static string Url(string baseUrl, string path)
        => UrlBuilder.Base(baseUrl).Path(path).Build();
}