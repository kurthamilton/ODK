using ODK.Services.Caching;
using ODK.Web.Common.Chapters;

namespace ODK.Web.Razor.Pages.Chapters;

public class AboutModel : ChapterPageModel2<AboutPageViewModel>
{
    private readonly IChapterWebService _chapterWebService;

    public AboutModel(IChapterWebService chapterWebService)
    {
        _chapterWebService = chapterWebService;
    }

    protected override Task<AboutPageViewModel> GetViewModelAsync() => _chapterWebService.GetAboutPageViewModelAsync(MemberId, Name);
}
