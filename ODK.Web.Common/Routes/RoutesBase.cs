using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Web.Common.Routes;

public abstract class RoutesBase
{
    protected string GetRoute(PlatformType platform, Chapter chapter, string path)
        => $"{BaseUrl(platform, chapter)}{PathSuffix(path)}";

    protected string GetRoute(Chapter? chapter, string path)
        => $"{(chapter != null ? BaseUrl(PlatformType.DrunkenKnitwits, chapter) : string.Empty)}{PathSuffix(path)}";

    private string BaseUrl(PlatformType platform, Chapter chapter) => platform switch
    {
        PlatformType.DrunkenKnitwits => $"/{chapter.GetDisplayName(platform).ToLowerInvariant()}",
        _ => $"/groups/{chapter.Slug}"
    };

    private string PathSuffix(string path)
        => !string.IsNullOrEmpty(path)
            ? $"{path}"
            : string.Empty;
}
