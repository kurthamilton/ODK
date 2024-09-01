using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Services.Members.ViewModels;

public class AdminMemberAdminPageViewModel
{
    public required ChapterAdminMember AdminMember { get; init; }

    public required Chapter Chapter { get; init; }

    public required PlatformType Platform { get; init; }
}
