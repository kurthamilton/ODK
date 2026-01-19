using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Services.Chapters.ViewModels;

namespace ODK.Services.Chapters;

public interface IChapterViewModelService
{
    Task<GroupsViewModel> FindGroups(
        PlatformType platform, Guid? currentMemberId, GroupFilter filter);

    Task<AccountMenuChaptersViewModel> GetAccountMenuChaptersViewModel(MemberServiceRequest request);

    Task<ChapterAboutPageViewModel> GetChapterAboutPage(Chapter chapter);

    Task<ChapterCreateViewModel> GetChapterCreate(PlatformType platform, Guid currentMemberId);

    Task<GroupContactPageViewModel> GetGroupContactPage(
        ServiceRequest request, Guid? currentMemberId, Chapter chapter);

    Task<GroupConversationPageViewModel> GetGroupConversationPage(
        MemberServiceRequest request, Chapter chapter, Guid conversationId);

    Task<GroupEventsPageViewModel> GetGroupEventsPage(ServiceRequest request, Guid? currentMemberId, Chapter chapter);

    Task<GroupPageViewModel> GetGroupPage(ServiceRequest request, Guid? currentMemberId, Chapter chapter);

    Task<GroupEventsPageViewModel> GetGroupPastEventsPage(ServiceRequest request, Guid? currentMemberId, Chapter chapter);

    Task<GroupHomePageViewModel> GetGroupHomePage(ServiceRequest request, Guid? currentMemberId, Chapter chapter);

    Task<GroupJoinPageViewModel> GetGroupJoinPage(ServiceRequest request, Guid? currentMemberId, Chapter chapter);

    Task<GroupProfilePageViewModel> GetGroupProfilePage(MemberServiceRequest request, Chapter chapter);

    Task<GroupQuestionsPageViewModel> GetGroupQuestionsPage(ServiceRequest request, Guid? currentMemberId, Chapter chapter);

    Task<GroupSubscriptionPageViewModel> GetGroupSubscriptionPage(
        MemberServiceRequest request, Chapter chapter);

    Task<ChapterHomePageViewModel> GetHomePage(ServiceRequest request, Guid? currentMemberId, Chapter chapter);

    Task<MemberChaptersViewModel> GetMemberChapters(MemberServiceRequest request);
}