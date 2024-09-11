using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterLinksAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required ChapterLinks? Links { get; init; }

    public required MemberSiteSubscription? OwnerSubscription { get; init; }

    public required PlatformType Platform { get; init; }

    public required bool ShowInstagramFeed { get; init; }
}
