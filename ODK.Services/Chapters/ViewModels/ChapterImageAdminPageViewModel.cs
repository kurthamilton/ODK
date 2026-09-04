using ODK.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterImageAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    /// <summary>
    /// Version of the stored picture, or null when the group has none. The page renders the picture from
    /// its own url, so the bytes never travel with the page.
    /// </summary>
    public required int? ImageVersion { get; init; }
}
