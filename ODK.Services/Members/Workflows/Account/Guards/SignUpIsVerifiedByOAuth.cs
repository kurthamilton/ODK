using ODK.Core.Workflows;

namespace ODK.Services.Members.Workflows.Account.Guards;

public sealed class SignUpIsVerifiedByOAuth : IGuard<AccountContext>
{
    public string Description => "verified by OAuth";

    public bool IsSatisfied(AccountContext context) => context.VerifiedByOAuth;
}
