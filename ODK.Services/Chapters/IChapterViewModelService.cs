using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Services.Chapters.ViewModels;

namespace ODK.Services.Chapters;

public interface IChapterViewModelService
{
    Task<GroupsViewModel> FindGroups(
        PlatformType platform, Member? currentMember, GroupFilter filter);

    Task<AccountMenuChaptersViewModel> GetAccountMenuChaptersViewModel(MemberServiceRequest request);

    Task<ChapterAboutPageViewModel> GetChapterAboutPage(Chapter chapter);

    Task<ChapterCreateViewModel> GetChapterCreate(PlatformType platform, Member currentMember);

    Task<GroupContactPageViewModel> GetGroupContactPage(
        ServiceRequest request, Member? currentMember, Chapter chapter);

    Task<GroupConversationPageViewModel> GetGroupConversationPage(
        MemberChapterServiceRequest request, Guid conversationId);

    Task<GroupEventsPageViewModel> GetGroupEventsPage(
        ServiceRequest request, Member? currentMember, Chapter chapter);

    Task<GroupPageViewModel> GetGroupPage(
        ServiceRequest request, Member? currentMember, Chapter chapter);

    Task<GroupEventsPageViewModel> GetGroupPastEventsPage(
        ServiceRequest request, Member? currentMember, Chapter chapter);

    Task<GroupHomePageViewModel> GetGroupHomePage(
        ServiceRequest request, Member? currentMember, Chapter chapter);

    Task<GroupJoinPageViewModel> GetGroupJoinPage(
        ServiceRequest request, Member? currentMember, Chapter chapter);

    Task<GroupProfilePageViewModel> GetGroupProfilePage(MemberChapterServiceRequest request);

    Task<GroupQuestionsPageViewModel> GetGroupQuestionsPage(
        ServiceRequest request, Member? currentMember, Chapter chapter);

    Task<GroupSubscriptionPageViewModel> GetGroupSubscriptionPage(
        MemberChapterServiceRequest request);

    Task<ChapterHomePageViewModel> GetHomePage(
        ServiceRequest request, Member? currentMember, Chapter chapter);

    Task<MemberChaptersViewModel> GetMemberChapters(MemberServiceRequest request);
}