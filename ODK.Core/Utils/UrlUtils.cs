namespace ODK.Core.Utils;

public static class UrlUtils
{
    public static string BaseUrl(string url)
    {
        var uri = new Uri(url);
        return uri.GetLeftPart(UriPartial.Authority);
    }
}
