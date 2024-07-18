using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Core.Events;
using ODK.Services.Events;
using ODK.Web.Common.Chapters;

namespace ODK.Web.Razor.Pages.Chapters.Events;

public abstract class EventPageModel : ChapterPageModel2<EventPageViewModel>
{
    private readonly IChapterWebService _chapterWebService;

    protected EventPageModel(IChapterWebService chapterWebService, IEventService eventService) 
    {
        _chapterWebService = chapterWebService;

        EventService = eventService;
    }

    protected IEventService EventService { get; }

    private Guid EventId { get; set; }

    public async Task<IActionResult> OnGet(Guid id, string? rsvp = null)
    {
        if (MemberId != null && 
            !string.IsNullOrEmpty(rsvp) && 
            Enum.TryParse<EventResponseType>(rsvp, true, out var response))
        {
            try
            {                
                await EventService.UpdateMemberResponse(MemberId.Value, EventId, response);
            }
            catch
            {
                // do nothing
            }

            return RedirectToSelf(id);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, EventResponseType responseType)
    {
        if (MemberId != null)
        {
            await EventService.UpdateMemberResponse(MemberId.Value, id, responseType);
        }

        return RedirectToSelf(id);
    }

    protected override Task<EventPageViewModel> GetViewModelAsync() 
        => _chapterWebService.GetEventPageViewModelAsync(MemberId, Name, EventId);

    protected override void OnBeforeGetViewModel(PageHandlerExecutingContext context)
    {
        if (!Guid.TryParse(context.RouteData.Values["id"] as string, out var eventId))
        {
            context.Result = NotFound();
            return;
        }

        EventId = eventId;

        base.OnBeforeGetViewModel(context);
    }

    protected abstract IActionResult RedirectToSelf(Guid id);
}
