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

    Task<ChapterCreateViewModel> GetChapterCreateViewModel(MemberServiceRequest request);

    Task<GroupContactPageViewModel> GetGroupContactPage(
        ChapterServiceRequest request, Member? currentMember);

    Task<GroupConversationPageViewModel> GetGroupConversationPage(
        MemberChapterServiceRequest request, Guid conversationId);

    Task<GroupEventsPageViewModel> GetGroupEventsPage(
        ChapterServiceRequest request, Member? currentMember);

    Task<GroupPageViewModel> GetGroupPage(
        ChapterServiceRequest request, Member? currentMember);

    Task<GroupEventsPageViewModel> GetGroupPastEventsPage(
        ChapterServiceRequest request, Member? currentMember);

    Task<GroupHomePageViewModel> GetGroupHomePage(
        ChapterServiceRequest request, Member? currentMember);

    Task<GroupJoinPageViewModel> GetGroupJoinPage(
        ChapterServiceRequest request, Member? currentMember);

    Task<GroupProfilePageViewModel> GetGroupProfilePage(MemberChapterServiceRequest request);

    Task<GroupQuestionsPageViewModel> GetGroupQuestionsPage(
        ChapterServiceRequest request, Member? currentMember);

    Task<GroupSubscriptionPageViewModel> GetGroupSubscriptionPage(
        MemberChapterServiceRequest request);

    Task<ChapterHomePageViewModel> GetHomePage(
        ChapterServiceRequest request, Member? currentMember);

    Task<MemberChaptersViewModel> GetMemberChapters(MemberServiceRequest request);
}