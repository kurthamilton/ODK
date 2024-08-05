namespace ODK.Web.Common.Routes;

public abstract class RoutesBase
{
    protected string GetRoute(string? chapterName, string path)
        => (!string.IsNullOrEmpty(chapterName) ? $"/{chapterName}/" : "/") + path;
}
