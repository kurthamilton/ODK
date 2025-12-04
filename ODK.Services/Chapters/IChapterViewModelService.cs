using ODK.Core.Platforms;
using ODK.Services.Chapters.ViewModels;

namespace ODK.Services.Chapters;

public interface IChapterViewModelService
{
    Task<GroupsViewModel> FindGroups(
        PlatformType platform, Guid? currentMemberId, GroupFilter filter);

    Task<ChapterCreateViewModel> GetChapterCreate(PlatformType platform, Guid currentMemberId);

    Task<GroupContactPageViewModel> GetGroupContactPage(
        ServiceRequest request, Guid? currentMemberId, string slug);

    Task<GroupConversationPageViewModel> GetGroupConversationPage(
        MemberServiceRequest request, string slug, Guid conversationId);

    Task<GroupEventsPageViewModel> GetGroupEventsPage(ServiceRequest request, Guid? currentMemberId, string slug);

    Task<GroupPageViewModel> GetGroupPage(ServiceRequest request, Guid? currentMemberId, string slug);

    Task<GroupEventsPageViewModel> GetGroupPastEventsPage(ServiceRequest request, Guid? currentMemberId, string slug);

    Task<GroupHomePageViewModel> GetGroupHomePage(ServiceRequest request, Guid? currentMemberId, string slug);

    Task<GroupJoinPageViewModel> GetGroupJoinPage(ServiceRequest request, Guid? currentMemberId, string slug);

    Task<GroupProfilePageViewModel> GetGroupProfilePage(MemberServiceRequest request, string slug);    

    Task<GroupQuestionsPageViewModel> GetGroupQuestionsPage(ServiceRequest request, Guid? currentMemberId, string slug);

    Task<GroupSubscriptionPageViewModel> GetGroupSubscriptionPage(
        MemberServiceRequest request, string slug);

    Task<ChapterHomePageViewModel> GetHomePage(ServiceRequest request, Guid? currentMemberId, string chapterName);

    Task<MemberChaptersViewModel> GetMemberChapters(MemberServiceRequest request);
}
