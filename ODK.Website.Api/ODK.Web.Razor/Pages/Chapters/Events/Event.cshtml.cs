using Microsoft.AspNetCore.Mvc;
using ODK.Core.Events;
using ODK.Services.Caching;
using ODK.Services.Events;

namespace ODK.Web.Razor.Pages.Chapters.Events
{
    public class EventModel : ChapterPageModel
    {
        private readonly IEventService _eventService;

        public EventModel(IRequestCache requestCache, IEventService eventService)
            : base(requestCache)
        {
            _eventService = eventService;
        }

        public Event Event { get; private set; } = null!;

        public async Task<IActionResult> OnGet(Guid id)
        {
            Event = await _eventService.GetEvent(Chapter?.Id ?? Guid.Empty, id);
            if (Event == null)
            {
                return RedirectToPage($"/{Chapter?.Name}/Events");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid id, EventResponseType responseType)
        {
            if (CurrentMember != null)
            {
                await _eventService.UpdateMemberResponse(CurrentMember.Id, id, responseType);
            }

            return Redirect($"/{Chapter!.Name}/Events/{id}");
        }
    }
}
