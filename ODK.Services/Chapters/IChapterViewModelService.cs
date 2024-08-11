using ODK.Core.Countries;
using ODK.Services.Chapters.ViewModels;

namespace ODK.Services.Chapters;

public interface IChapterViewModelService
{
    Task<ChapterCreateViewModel> GetChapterCreate(Guid currentMemberId);

    Task<GroupHomePageViewModel> GetGroupHomePage(Guid? currentMemberId, string slug);

    Task<GroupsViewModel> GetGroups(LatLong location);

    Task<ChapterHomePageViewModel> GetHomePage(Guid? currentMemberId, string chapterName);

    Task<MemberChaptersViewModel> GetMemberChapters(Guid memberId);
}
