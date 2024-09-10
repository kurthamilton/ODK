using ODK.Services.Chapters.ViewModels;

namespace ODK.Services.Chapters;

public interface IChapterViewModelService
{
    Task<GroupsViewModel> FindGroups(Guid? currentMemberId, GroupFilter filter);

    Task<ChapterCreateViewModel> GetChapterCreate(Guid currentMemberId);

    Task<GroupContactPageViewModel> GetGroupContactPage(Guid? currentMemberId, string slug);

    Task<GroupConversationPageViewModel> GetGroupConversationPage(Guid currentMemberId, string slug, Guid conversationId);

    Task<GroupEventsPageViewModel> GetGroupEventsPage(Guid? currentMemberId, string slug);

    Task<GroupEventsPageViewModel> GetGroupPastEventsPage(Guid? currentMemberId, string slug);

    Task<GroupHomePageViewModel> GetGroupHomePage(Guid? currentMemberId, string slug);

    Task<GroupJoinPageViewModel> GetGroupJoinPage(Guid? currentMemberId, string slug);

    Task<GroupProfilePageViewModel> GetGroupProfilePage(Guid currentMemberId, string slug);

    Task<GroupProfileSubscriptionPageViewModel> GetGroupProfileSubscriptionPage(Guid currentMemberId, string slug);

    Task<GroupQuestionsPageViewModel> GetGroupQuestionsPage(Guid? currentMemberId, string slug);

    Task<ChapterHomePageViewModel> GetHomePage(Guid? currentMemberId, string chapterName);

    Task<MemberChaptersViewModel> GetMemberChapters(Guid memberId);
}
