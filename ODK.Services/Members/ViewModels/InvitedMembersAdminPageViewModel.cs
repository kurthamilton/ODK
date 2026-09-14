using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Services.Members.ViewModels;

public class InvitedMembersAdminPageViewModel
{
    /// <summary>
    /// Invited people with no account here yet, so accepting is also what raises one. The number a group
    /// most wants back from an import, and the one it cannot work out from the list.
    /// </summary>
    /// <remarks>
    /// Derived from the invites rather than recorded when they were raised, so it stays right after a
    /// purge, after a member activates, and for a group that imported in three goes.
    /// </remarks>
    public int AwaitingActivation => Invited.Count(x => !x.Member.Activated);

    /// <summary>
    /// Whether sending is offered: the group holds invites it has not emailed, and it is published, so the
    /// link an invite carries lands somewhere. Who may send them is the page's securable, not this.
    /// </summary>
    public bool CanSend => Held > 0 && Chapter.IsPublished();

    public required Chapter Chapter { get; init; }

    /// <summary>How many of the invites have yet to be emailed.</summary>
    public int Held => Invited.Count(x => x.SentUtc == null);

    /// <summary>Invited people who already had an account here.</summary>
    public int ExistingAccounts => Invited.Count(x => x.Member.Activated);

    public required IReadOnlyCollection<InvitedMemberViewModel> Invited { get; init; }

    public required PlatformType Platform { get; init; }

    public required int RetentionDays { get; init; }
}
