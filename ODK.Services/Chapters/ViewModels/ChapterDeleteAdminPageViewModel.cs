using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterDeleteAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required int MemberCount { get; init; }

    public required PlatformType Platform { get; init; }
}
