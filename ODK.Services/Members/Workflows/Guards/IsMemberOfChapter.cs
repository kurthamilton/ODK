using ODK.Core.Workflows;

namespace ODK.Services.Members.Workflows.Guards;

public sealed class IsMemberOfChapter : IGuard<AccountContext>
{
    public string Description => "a member of the group";

    public bool IsSatisfied(AccountContext context) =>
        context.Member?.IsMemberOf(context.ChapterId) == true;
}
