using ODK.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterHeaderImageAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required ChapterHeaderImage? Image { get; init; }
}
