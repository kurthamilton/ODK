namespace ODK.Services.Members.Workflows;

/// <summary>
/// Where someone stands with respect to an account and a group. Every state is derived from the
/// domain - an outstanding activation token, an invitation, a membership row - and never stored.
/// </summary>
/// <remarks>
/// Numbered because a state or a trigger can travel as a background job argument, which Hangfire
/// serialises as the number: renumbering would have a job queued by one version run as something else
/// under the next.
/// </remarks>
public enum AccountState
{
    None = 0,

    /// <summary>No account exists for the address.</summary>
    Anonymous = 1,

    /// <summary>An admin has asked the address to join a group. Nobody has signed up against it.</summary>
    Invited = 2,

    /// <summary>An account exists but has never been activated, so an activation token is outstanding.</summary>
    Registered = 3,

    /// <summary>The account has a password and can sign in. It belongs to no group.</summary>
    Activated = 4,

    /// <summary>They have applied to a group that approves new members, and an admin has yet to decide.</summary>
    PendingApproval = 5,

    /// <summary>An approved member of the group.</summary>
    GroupMember = 6
}
