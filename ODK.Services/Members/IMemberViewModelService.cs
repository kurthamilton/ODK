using ODK.Services.Members.ViewModels;

namespace ODK.Services.Members;

public interface IMemberViewModelService
{
    Task<MemberConversationsPageViewModel> GetMemberConversationsPage(IMemberServiceRequest request);

    Task<MemberConversationsPageViewModel> GetMemberConversationsPage(IMemberChapterServiceRequest request);

    Task<MemberInterestsPageViewModel> GetMemberInterestsPage(Guid currentMemberId);

    Task<MemberPageViewModel> GetMemberPage(IMemberChapterServiceRequest request, Guid memberId);

    Task<MembersPageViewModel> GetMembersPage(IMemberChapterServiceRequest request);
}