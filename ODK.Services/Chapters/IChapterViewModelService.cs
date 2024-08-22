using ODK.Core.Countries;
using ODK.Services.Chapters.ViewModels;

namespace ODK.Services.Chapters;

public interface IChapterViewModelService
{
    Task<GroupsViewModel> FindGroups(ILocation location, Distance radius);

    Task<GroupsViewModel> FindGroups(Guid currentMemberId, Distance radius);

    Task<ChapterCreateViewModel> GetChapterCreate(Guid currentMemberId);

    Task<GroupHomePageViewModel> GetGroupHomePage(Guid? currentMemberId, string slug);

    Task<GroupJoinPageViewModel> GetGroupJoinPage(Guid? currentMemberId, string slug);

    Task<GroupProfilePageViewModel> GetGroupProfilePage(Guid currentMemberId, string slug);

    Task<GroupProfileSubscriptionPageViewModel> GetGroupProfileSubscriptionPage(Guid currentMemberId, string slug);

    Task<GroupQuestionsPageViewModel> GetGroupQuestionsPage(Guid? currentMemberId, string slug);

    Task<ChapterHomePageViewModel> GetHomePage(Guid? currentMemberId, string chapterName);

    Task<MemberChaptersViewModel> GetMemberChapters(Guid memberId);
}
