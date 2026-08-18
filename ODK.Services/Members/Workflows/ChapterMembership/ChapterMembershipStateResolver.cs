using ODK.Core.Workflows;

namespace ODK.Services.Members.Workflows.ChapterMembership;

/// <summary>
/// Derives what the member is to the group. Nothing stores it: a membership row, or failing that an
/// outstanding invitation, is the whole answer.
/// </summary>
public sealed class ChapterMembershipStateResolver
    : IStateResolver<ChapterMembershipState, ChapterMembershipContext>
{
    public ChapterMembershipState Resolve(ChapterMembershipContext context)
    {
        var memberChapter = context.Member.MemberChapter(context.ChapterId);
        if (memberChapter != null)
        {
            return memberChapter.Approved
                ? ChapterMembershipState.Joined
                : ChapterMembershipState.PendingApproval;
        }

        return context.Invite != null
            ? ChapterMembershipState.Invited
            : ChapterMembershipState.NotJoined;
    }
}
