using ODK.Core.Workflows;

namespace ODK.Services.Members.Workflows.ChapterMembership.Guards;

public sealed class ApprovalIsRequired : IGuard<ChapterMembershipContext>
{
    public string Description => "requiring approval";

    public bool IsSatisfied(ChapterMembershipContext context) => context.ApprovalRequired;
}
