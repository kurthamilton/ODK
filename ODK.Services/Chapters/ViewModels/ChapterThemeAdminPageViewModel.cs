using ODK.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterThemeAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    /// <summary>
    /// Whether the theme can be changed. A group whose owner doesn't have the theme feature still keeps
    /// and renders the theme it already has - only editing is withheld - so the page shows the current
    /// values read-only rather than hiding them.
    /// </summary>
    public required bool CanEdit { get; init; }
}
