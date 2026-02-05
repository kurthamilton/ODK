using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Services.Chapters.ViewModels;

namespace ODK.Services.Chapters;

public interface IChapterViewModelService
{
    Task<GroupsViewModel> FindGroups(
        PlatformType platform, Member? currentMember, GroupFilter filter);

    Task<AccountMenuChaptersViewModel> GetAccountMenuChaptersViewModel(IMemberServiceRequest request);

    Task<ChapterAboutPageViewModel> GetChapterAboutPage(Chapter chapter);

    Task<ChapterCreateViewModel> GetChapterCreateViewModel(IMemberServiceRequest request);

    Task<GroupContactPageViewModel> GetGroupContactPage(
        IChapterServiceRequest request, Member? currentMember);

    Task<GroupConversationPageViewModel> GetGroupConversationPage(
        IMemberChapterServiceRequest request, Guid conversationId);

    Task<GroupEventsPageViewModel> GetGroupEventsPage(
        IChapterServiceRequest request, Member? currentMember);

    Task<GroupPageViewModel> GetGroupPage(
        IChapterServiceRequest request, Member? currentMember);

    Task<GroupEventsPageViewModel> GetGroupPastEventsPage(
        IChapterServiceRequest request, Member? currentMember);

    Task<GroupHomePageViewModel> GetGroupHomePage(
        IChapterServiceRequest request, Member? currentMember);

    Task<GroupJoinPageViewModel> GetGroupJoinPage(
        IChapterServiceRequest request, Member? currentMember);

    Task<GroupProfilePageViewModel> GetGroupProfilePage(IMemberChapterServiceRequest request);

    Task<GroupQuestionsPageViewModel> GetGroupQuestionsPage(
        IChapterServiceRequest request, Member? currentMember);

    Task<GroupSubscriptionPageViewModel> GetGroupSubscriptionPage(
        IMemberChapterServiceRequest request);

    Task<ChapterHomePageViewModel> GetHomePage(
        IChapterServiceRequest request, Member? currentMember);

    Task<MemberChaptersViewModel> GetMemberChapters(IMemberServiceRequest request);
}