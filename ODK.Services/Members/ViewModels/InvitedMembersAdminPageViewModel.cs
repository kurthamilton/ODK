using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Services.Members.ViewModels;

public class InvitedMembersAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required IReadOnlyCollection<InvitedMemberViewModel> Invited { get; init; }

    public required PlatformType Platform { get; init; }

    public required int RetentionDays { get; init; }
}
