namespace ODK.Services.Members.Models;

/// <summary>
/// What the accept-invitation form submits. An invited member already has the account an import raised, so
/// this is not a sign-up: it is the first password on that account, the name they confirmed, and the group's
/// answers - all in one act.
/// </summary>
public class InvitationAcceptModel
{
    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    /// <summary>The first password on the account, which is what makes it able to sign in.</summary>
    public required string Password { get; init; }

    /// <summary>The member's answers to the group's questions.</summary>
    public required IReadOnlyCollection<MemberPropertyUpdateModel> Properties { get; init; }

    /// <summary>
    /// The token the invitation link carried, which is what names the invitation being accepted. Holding it
    /// proves the link reached the address the import supplied, so no activation email is sent for an account
    /// accepted this way.
    /// </summary>
    public required string Token { get; init; }
}
