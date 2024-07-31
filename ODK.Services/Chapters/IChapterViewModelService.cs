using ODK.Services.Chapters.ViewModels;

namespace ODK.Services.Chapters;

public interface IChapterViewModelService
{
    Task<ChapterHomePageViewModel> GetHomePage(Guid? currentMemberId, string chapterName);
}
