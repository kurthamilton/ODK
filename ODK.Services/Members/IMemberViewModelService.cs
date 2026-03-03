using ODK.Services.Members.ViewModels;

namespace ODK.Services.Members;

public interface IMemberViewModelService
{
    Task<MemberConversationPageViewModel> GetMemberConversationPage(IMemberServiceRequest request, Guid conversationId);

    Task<MemberConversationsPageViewModel> GetMemberConversationsPage(IMemberServiceRequest request, bool archived);

    Task<MemberConversationsPageViewModel> GetMemberConversationsPage(IMemberChapterServiceRequest request, bool archived);

    Task<MemberInterestsPageViewModel> GetMemberInterestsPage(IMemberServiceRequest request);

    Task<MemberPageViewModel> GetMemberPage(IMemberChapterServiceRequest request, Guid memberId);

    Task<MembersPageViewModel> GetMembersPage(IMemberChapterServiceRequest request);
}