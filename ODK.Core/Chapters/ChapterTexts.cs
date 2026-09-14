using ODK.Core.Platforms;

namespace ODK.Core.Chapters;

public class ChapterTexts : IChapterEntity
{
    public Guid ChapterId { get; set; }

    public string? DescriptionHtml { get; set; }

    public string RegisterTextHtml { get; set; } = string.Empty;

    public string? ShortDescription { get; set; }

    public string WelcomeTextHtml { get; set; } = string.Empty;

    /// <summary>
    /// Whether a platform shows a group's short description. It is the summary a group is listed by, and
    /// only Group Squirrel lists groups - so anywhere else it would be asked for and then never read.
    /// </summary>
    public static bool ShowsShortDescription(PlatformType platform)
        => platform == PlatformType.GroupSquirrel;
}