using ODK.Core.Chapters;
using ODK.Core.Pages;
using ODK.Core.Platforms;

namespace ODK.Web.Common.Routes;

/// <summary>
/// A page an anonymous visitor may open, paired with the conditions under which it exists. Enumerating
/// these is what lets callers answer "which pages should a crawler be told about?" without hard-coding a
/// list.
/// </summary>
/// <remarks>
/// The conditions are declared rather than applied, because the two callers need different amounts of
/// them: a sitemap resolves every one of them against a group's settings, while the test that checks the
/// registry against the pages that exist needs the routes themselves and knows nothing about any group.
/// </remarks>
public class PublicRoute
{
    /// <summary>
    /// The page a group hides by hiding this <see cref="Core.Pages.PageType"/>, where hiding it 404s the
    /// page rather than only dropping it from the menu.
    /// </summary>
    public PageType? ChapterPage { get; init; }

    /// <summary>
    /// The feature whose visibility governs the page. Anonymous viewers resolve to
    /// <see cref="ChapterFeatureVisibilityType.Public"/>, so the page is theirs to open only where the
    /// group has set this feature to Public.
    /// </summary>
    public ChapterFeatureType? Feature { get; init; }

    public required string Path { get; init; }

    /// <summary>Null where the page exists on both platforms.</summary>
    public PlatformType? Platform { get; init; }

    /// <summary>The page exists only where the group has questions to show on it.</summary>
    public bool RequiresQuestions { get; init; }

    public bool ExistsOn(PlatformType platform) => Platform == null || Platform == platform;
}
