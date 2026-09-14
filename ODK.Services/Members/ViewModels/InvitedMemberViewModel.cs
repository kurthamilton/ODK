using ODK.Core.Members;

namespace ODK.Services.Members.ViewModels;

public class InvitedMemberViewModel
{
    /// <summary>
    /// Whether emailing this invite again is offered: the group is published so the link lands somewhere,
    /// the invite has been sent once, and the cooldown since has passed.
    /// </summary>
    public required bool CanResend { get; init; }

    public required int DaysRemaining { get; init; }

    public required DateTime DeletedUtc { get; init; }

    public required DateTime InvitedUtc { get; init; }

    public required Member Member { get; init; }

    /// <summary>When the invite was emailed, or null while the group is still holding it.</summary>
    public required DateTime? SentUtc { get; init; }
}
