using ODK.Core.Countries;
using ODK.Services.Chapters.ViewModels;

namespace ODK.Services.Chapters;

public interface IChapterViewModelService
{
    Task<GroupsViewModel> FindGroups(LatLong location, string locationName);

    Task<GroupsViewModel> FindGroups(Guid currentMemberId);

    Task<ChapterCreateViewModel> GetChapterCreate(Guid currentMemberId);

    Task<GroupHomePageViewModel> GetGroupHomePage(Guid? currentMemberId, string slug);    

    Task<ChapterHomePageViewModel> GetHomePage(Guid? currentMemberId, string chapterName);

    Task<MemberChaptersViewModel> GetMemberChapters(Guid memberId);
}
