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

    public static string Url(string baseUrl, string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return baseUrl;
        }

        if (!path.StartsWith('/'))
        {
            path = $"/{path}";
        }

        return baseUrl + path;
    }
}