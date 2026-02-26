using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;

namespace ODK.Services.Members.ViewModels;

public class AdminMembersAdminPageViewModel
{
    public required IReadOnlyCollection<ChapterAdminMember> AdminMembers { get; init; }

    public required Chapter Chapter { get; init; }

    public required IReadOnlyCollection<Member> Members { get; init; }

    public required SiteSubscription? OwnerSubscription { get; init; }

    public required PlatformType Platform { get; init; }
}
