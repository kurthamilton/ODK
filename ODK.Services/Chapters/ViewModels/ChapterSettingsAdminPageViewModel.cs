using ODK.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

/// <summary>
/// Every group setting the page holds, one property per block of them. A block is null where the member
/// may not open it or the platform does not offer it, so the page renders only what it can act on.
/// </summary>
public class ChapterSettingsAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required ChapterImageAdminPageViewModel? Image { get; init; }

    public required ChapterLinksAdminPageViewModel? Links { get; init; }

    public required ChapterLocationAdminPageViewModel? Location { get; init; }

    public required ChapterPagesAdminPageViewModel? Pages { get; init; }

    public required ChapterPrivacyAdminPageViewModel? Privacy { get; init; }

    public required ChapterThemeAdminPageViewModel? Theme { get; init; }

    public required ChapterTopicsAdminPageViewModel? Topics { get; init; }
}
