using ODK.Core.Members;

namespace ODK.Services.Members.ViewModels;

public class InvitedMemberViewModel
{
    public required int DaysRemaining { get; init; }

    public required DateTime DeletedUtc { get; init; }

    public required DateTime InvitedUtc { get; init; }

    public required Member Member { get; init; }
}
