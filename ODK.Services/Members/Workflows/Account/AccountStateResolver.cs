using ODK.Core.Workflows;

namespace ODK.Services.Members.Workflows.Account;

/// <summary>
/// Derives the account state from the domain. Nothing stores it, so this is the only thing that decides what
/// state an account is in.
/// </summary>
public sealed class AccountStateResolver : IStateResolver<AccountState, AccountContext>
{
    public AccountState Resolve(AccountContext context)
    {
        var member = context.Member;
        if (member == null)
        {
            return AccountState.Anonymous;
        }

        return member.Activated ? AccountState.Activated : AccountState.Registered;
    }
}
