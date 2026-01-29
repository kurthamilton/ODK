using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Web.Common.Routes;

public abstract class RoutesBase
{
    protected RoutesBase(PlatformType platform)
    {
        Platform = platform;
    }

    protected PlatformType Platform { get; }

    protected string GetRoute(Chapter? chapter, string path)
        => $"{BaseUrl(chapter)}{PathSuffix(path)}";

    private string BaseUrl(Chapter? chapter) => chapter != null
        ? Platform switch
        {
            PlatformType.DrunkenKnitwits => $"/{chapter.ShortName.ToLowerInvariant()}",
            _ => $"/groups/{chapter.Slug}"
        }
        : "/";

    private string PathSuffix(string path)
        => !string.IsNullOrEmpty(path)
            ? $"{path}"
            : string.Empty;
}
