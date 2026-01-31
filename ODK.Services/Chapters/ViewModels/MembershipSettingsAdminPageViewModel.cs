using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Services.Chapters.ViewModels;

public class MembershipSettingsAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required ChapterMembershipSettings MembershipSettings { get; init; }

    public required MemberSiteSubscription? OwnerSubscription { get; init; }
}
