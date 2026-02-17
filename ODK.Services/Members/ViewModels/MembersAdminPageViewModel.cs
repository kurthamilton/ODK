using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Data.Core.Members;

namespace ODK.Services.Members.ViewModels;

public class MembersAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required IReadOnlyCollection<MemberEmailPreference> MemberEventEmailPreferences { get; init; }

    public required IReadOnlyCollection<MemberWithAvatarDto> Members { get; init; }

    public required ChapterMembershipSettings? MembershipSettings { get; init; }

    public required PlatformType Platform { get; init; }

    public required IReadOnlyCollection<MemberSubscription> Subscriptions { get; init; }
}
