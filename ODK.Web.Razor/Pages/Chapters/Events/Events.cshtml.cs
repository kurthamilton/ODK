using ODK.Web.Common.Chapters;

namespace ODK.Web.Razor.Pages.Chapters.Events;

public class EventsModel : ChapterPageModel2<EventsPageViewModel>
{
    private readonly IChapterWebService _chapterWebService;

    public EventsModel(IChapterWebService chapterWebService)
    {
        _chapterWebService = chapterWebService;
    }

    protected override Task<EventsPageViewModel> GetViewModelAsync() => _chapterWebService.GetEventsPageViewModelAsync(MemberId, Name);
}
