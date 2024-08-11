using ODK.Core.Chapters;

namespace ODK.Core.Platforms;

public static class PlatformTypeExtensions
{
    public static string ChapterDisplayName(this PlatformType platform, Chapter chapter)
    {
        return platform == PlatformType.DrunkenKnitwits 
            ? $"{chapter.Name} Drunken Knitwits"
            : chapter.Name;
    }
}
