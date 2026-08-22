namespace ODK.Services.Members.Workflows.ChapterMembership;

/// <remarks>Numbered for the reason given on <see cref="ChapterMembershipState"/>.</remarks>
public enum ChapterMembershipTrigger
{
    None = 0,

    /// <summary>An admin imports the address, which asks the member to join.</summary>
    Invite = 1,

    /// <summary>
    /// The sign-up form is submitted on a platform where signing up to a group is joining it, which is
    /// Drunken Knitwits. Group Squirrel's sign-up touches no group, so it never fires this.
    /// </summary>
    SignUp = 2,

    /// <summary>A member who can already sign in asks to join.</summary>
    Join = 3,

    /// <summary>An admin approves an application.</summary>
    Approve = 4,

    /// <summary>
    /// An invited member follows their invitation link and accepts it, before their account can sign in.
    /// Distinct from <see cref="Join"/>, which any signed-in member fires: this one runs as a step of the
    /// account machine's transition that activates the account, so it stages writes and commits nothing.
    /// </summary>
    Accept = 5
}
