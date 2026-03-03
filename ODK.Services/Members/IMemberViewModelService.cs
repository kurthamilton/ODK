using ODK.Services.Members.ViewModels;

namespace ODK.Services.Members;

public interface IMemberViewModelService
{
    Task<MemberConversationPageViewModel> GetMemberConversationPage(IMemberServiceRequest request, Guid conversationId);

    Task<MemberConversationsPageViewModel> GetMemberConversationsPage(IMemberServiceRequest request);

    Task<MemberConversationsPageViewModel> GetMemberConversationsPage(IMemberChapterServiceRequest request);

    Task<MemberInterestsPageViewModel> GetMemberInterestsPage(IMemberServiceRequest request);

    Task<MemberPageViewModel> GetMemberPage(IMemberChapterServiceRequest request, Guid memberId);

    Task<MembersPageViewModel> GetMembersPage(IMemberChapterServiceRequest request);
}