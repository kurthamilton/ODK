using ODK.Core.Chapters;
using ODK.Services.Members.ViewModels;

namespace ODK.Services.Members;

public interface IMemberViewModelService
{
    Task<MemberConversationsPageViewModel> GetMemberConversationsPage(MemberServiceRequest request);

    Task<MemberConversationsPageViewModel> GetMemberConversationsPage(MemberChapterServiceRequest request);

    Task<MemberInterestsPageViewModel> GetMemberInterestsPage(Guid currentMemberId);

    Task<MemberPageViewModel> GetMemberPage(MemberChapterServiceRequest request, Guid memberId);

    Task<MembersPageViewModel> GetMembersPage(MemberChapterServiceRequest request);
}