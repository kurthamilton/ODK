using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;

namespace ODK.Services.Members.ViewModels;

public class MemberApprovalsAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required ChapterMembershipSettings? MembershipSettings { get; init; }

    public required IReadOnlyCollection<Member> Pending { get; init; }

    public required PlatformType Platform { get; init; }
}
