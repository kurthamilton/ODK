using ODK.Core.Workflows;

namespace ODK.Services.Members.Workflows.Guards;

public sealed class ApprovalIsRequired : IGuard<AccountContext>
{
    public string Description => "requiring approval";

    public bool IsSatisfied(AccountContext context) => context.ApprovalRequired;
}
