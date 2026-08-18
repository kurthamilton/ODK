using ODK.Core.Workflows;

namespace ODK.Services.Members.Workflows.Guards;

public sealed class MembershipIsApproved : IGuard<AccountContext>
{
    public string Description => "approved";

    public bool IsSatisfied(AccountContext context) =>
        context.Member?.IsApprovedMemberOf(context.ChapterId) == true;
}
