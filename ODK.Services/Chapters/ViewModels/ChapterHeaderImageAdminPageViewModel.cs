using ODK.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterHeaderImageAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    /// <summary>
    /// Version of the stored image, or null when the group has none. The page renders the image from its
    /// own url, so the bytes never travel with the page.
    /// </summary>
    public required int? ImageVersion { get; init; }
}
