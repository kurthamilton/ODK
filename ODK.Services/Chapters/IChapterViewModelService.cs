using ODK.Core.Chapters;
using ODK.Services.Chapters.ViewModels;

namespace ODK.Services.Chapters;

public interface IChapterViewModelService
{
    Task<GroupsViewModel> FindGroups(IServiceRequest request, GroupFilter filter);

    Task<AccountMenuChaptersViewModel> GetAccountMenuChaptersViewModel(IMemberServiceRequest request);

    Task<ChapterAboutPageViewModel> GetChapterAboutPage(Chapter chapter);

    Task<ChapterCreateViewModel> GetChapterCreateViewModel(IMemberServiceRequest request);

    Task<GroupContactPageViewModel> GetGroupContactPage(IChapterServiceRequest request);

    Task<GroupConversationPageViewModel> GetGroupConversationPage(
        IMemberChapterServiceRequest request, Guid conversationId);

    Task<GroupEventsPageViewModel> GetGroupEventsPage(IChapterServiceRequest request);

    Task<GroupPageViewModel> GetGroupPage(IChapterServiceRequest request);

    Task<GroupEventsPageViewModel> GetGroupPastEventsPage(IChapterServiceRequest request);

    Task<GroupHomePageViewModel> GetGroupHomePage(IChapterServiceRequest request);

    Task<GroupJoinPageViewModel> GetGroupJoinPage(IChapterServiceRequest request);

    Task<GroupProfilePageViewModel> GetGroupProfilePage(IMemberChapterServiceRequest request);

    Task<GroupQuestionsPageViewModel> GetGroupQuestionsPage(IChapterServiceRequest request);

    Task<GroupSubscriptionPageViewModel> GetGroupSubscriptionPage(IMemberChapterServiceRequest request);

    Task<ChapterHomePageViewModel> GetHomePage(IChapterServiceRequest request);

    Task<MemberChaptersViewModel> GetMemberChapters(IMemberServiceRequest request);
}