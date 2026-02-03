using ODK.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterImageAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required ChapterImage? Image { get; init; }
}
