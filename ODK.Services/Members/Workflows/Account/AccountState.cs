namespace ODK.Services.Members.Workflows.Account;

/// <summary>
/// Where an address stands with respect to an account. Site-level and chapter-free: what a member is to a
/// particular group is <see cref="ChapterMembership.ChapterMembershipState"/>, and the two are independent -
/// signing up on Drunken Knitwits writes a membership before the account can sign in.
/// </summary>
/// <remarks>
/// Numbered because a state or a trigger can travel as a background job argument, which Hangfire serialises
/// as the number: renumbering would have a job queued by one version run as something else under the next.
/// </remarks>
public enum AccountState
{
    None = 0,

    /// <summary>No account exists for the address.</summary>
    Anonymous = 1,

    /// <summary>An account exists but has never been activated, so it cannot sign in.</summary>
    Registered = 2,

    /// <summary>The account has a password and can sign in.</summary>
    Activated = 3
}
