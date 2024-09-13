using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterImageAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required ChapterImage? Image { get; init; }

    public required PlatformType Platform { get; init; }
}
