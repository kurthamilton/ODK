using ODK.Core.Chapters;
using ODK.Data.Core.Events;

namespace ODK.Services.Sitemap.ViewModels;

/// <summary>
/// One group, with everything that decides which of its pages an anonymous visitor can open.
/// </summary>
public class SitemapChapterViewModel
{
    public required Chapter Chapter { get; init; }

    public required IReadOnlyCollection<ChapterPage> ChapterPages { get; init; }

    /// <summary>
    /// The group's published events, empty where the group does not show its events publicly.
    /// </summary>
    public required IReadOnlyCollection<EventPublicationDto> Events { get; init; }

    public required bool HasQuestions { get; init; }

    public required ChapterPrivacySettings? PrivacySettings { get; init; }
}
