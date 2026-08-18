using ODK.Core.Members;
using ODK.Services.Members.Models;
using ODK.Services.Members.Workflows.Account;

namespace ODK.Services.Members.Workflows.ChapterMembership;

public interface IChapterMembershipContextFactory
{
    /// <summary>
    /// For an admin approving a queued member. Needs no query: the service has already loaded the member
    /// through the wrapper that enforces the securable.
    /// </summary>
    ChapterMembershipContext CreateForApproval(IChapterServiceRequest request, Member member);

    /// <summary>
    /// For a group sign-up, which creates the account and joins the group in one act. Needs no query: the
    /// account machine's context already holds everything, including the member it has just staged.
    /// </summary>
    ChapterMembershipContext CreateForGroupSignUp(AccountContext context);

    ChapterMembershipContext CreateForInvite(
        IChapterServiceRequest request,
        Member member,
        MemberChapterInvite? outstandingInvite);

    Task<ChapterMembershipContext> CreateForJoin(
        IMemberChapterServiceRequest request,
        IEnumerable<MemberPropertyUpdateModel> properties);
}
