namespace ODK.Services.Members.Workflows.ChapterMembership;

/// <summary>
/// What a member is to one group. Independent of whether their account can sign in - a Drunken Knitwits
/// sign-up writes the membership before activation, so a member can be <see cref="Joined"/> here while their
/// account is still <see cref="Account.AccountState.Registered"/>.
/// </summary>
/// <remarks>
/// Numbered because a state or a trigger can travel as a background job argument, which Hangfire serialises
/// as the number: renumbering would have a job queued by one version run as something else under the next.
/// </remarks>
public enum ChapterMembershipState
{
    None = 0,

    /// <summary>Nothing connects the member to the group.</summary>
    NotJoined = 1,

    /// <summary>An admin has asked them to join and they have not accepted.</summary>
    Invited = 2,

    /// <summary>They have applied to a group that approves new members, and an admin has yet to decide.</summary>
    PendingApproval = 3,

    /// <summary>An approved member of the group.</summary>
    Joined = 4
}
