using ODK.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterTextsAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    /// <summary>
    /// Whether the platform shows a short description, and so whether the form offers the box - see
    /// <see cref="ChapterTexts.ShowsShortDescription"/>.
    /// </summary>
    public required bool ShowShortDescription { get; init; }

    public required ChapterTexts? Texts { get; init; }
}
