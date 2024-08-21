namespace ODK.Web.Common.Routes;

public abstract class RoutesBase
{
    protected string GetRoute(string? chapterName, string path) 
        => BaseUrl(chapterName) + PathSuffix(path);

    private string BaseUrl(string? chapterName)
        => !string.IsNullOrEmpty(chapterName)
            ? $"/{chapterName}"
            : "";

    private string PathSuffix(string path)
        => !string.IsNullOrEmpty(path)
            ? $"/{path}"
            : "";
}
