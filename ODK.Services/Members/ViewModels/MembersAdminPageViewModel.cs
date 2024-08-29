using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;

namespace ODK.Services.Members.ViewModels;
public class MembersAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required IReadOnlyCollection<Member> Members { get; init; }

    public required PlatformType Platform { get; init; }

    public required IReadOnlyCollection<MemberSubscription> Subscriptions { get; init; }
}
