using ODK.Core.Workflows;

namespace ODK.Services.Members.Workflows.Account.Guards;

/// <summary>
/// Whether the sign-up presented the token from an invitation sent to the address being registered. Holding
/// it proves the sign-up reached that inbox, which is everything an activation email establishes - so the
/// account can be handed straight to setting a password and no activation email is sent.
/// </summary>
/// <remarks>
/// The token is trusted only against the account the submitted address resolves to: the invitation is read
/// off that account, so a token posted with any other address matches nothing.
/// </remarks>
public sealed class InviteTokenMatches : IGuard<AccountContext>
{
    public string Description => "presented with the invitation token";

    public bool IsSatisfied(AccountContext context) => context.PresentedTheInviteToken;
}
