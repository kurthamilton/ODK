using ODK.Web.Common.Chapters;

namespace ODK.Web.Razor.Pages.Chapters;

public class ChapterModel : ChapterPageModel2<ChapterPageViewModel>
{
    private readonly IChapterWebService _chapterWebService;

    public ChapterModel(IChapterWebService chapterWebService)
    {
        _chapterWebService = chapterWebService;
    }

    protected override Task<ChapterPageViewModel> GetViewModelAsync() 
        => _chapterWebService.GetChapterPageViewModelAsync(MemberId, Name);
}
