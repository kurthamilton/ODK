using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterLinksAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required ChapterLinks? Links { get; init; }

    public required PlatformType Platform { get; init; }
}
