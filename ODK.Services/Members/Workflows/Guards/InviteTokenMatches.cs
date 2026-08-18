using ODK.Core.Workflows;

namespace ODK.Services.Members.Workflows.Guards;

/// <summary>
/// Whether the sign-up presented the token from the invitation this address was sent. Holding it
/// proves the sign-up reached that inbox, which is everything an activation email establishes.
/// </summary>
public sealed class InviteTokenMatches : IGuard<AccountContext>
{
    public string Description => "presented with the invitation token";

    public bool IsSatisfied(AccountContext context) =>
        context.Invite != null &&
        !string.IsNullOrEmpty(context.InviteToken) &&
        context.Invite.Token == context.InviteToken;
}
