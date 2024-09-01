using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterTextsAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required PlatformType Platform { get; init; }

    public required ChapterTexts? Texts { get; init; }
}
