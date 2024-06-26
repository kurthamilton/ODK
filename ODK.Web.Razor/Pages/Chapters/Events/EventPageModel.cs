﻿using Microsoft.AspNetCore.Mvc;
using ODK.Core.Events;
using ODK.Services.Caching;
using ODK.Services.Events;

namespace ODK.Web.Razor.Pages.Chapters.Events;

public abstract class EventPageModel : ChapterPageModel
{
    protected EventPageModel(IRequestCache requestCache, IEventService eventService) 
        : base(requestCache)
    {
        EventService = eventService;
    }

    public Event Event { get; private set; } = null!;

    protected IEventService EventService { get; }

    public async Task<IActionResult> OnGet(Guid id, string? rsvp = null)
    {
        Event? @event = await EventService.GetEvent(Chapter.Id, id);
        if (@event == null)
        {
            return NotFound();
        }

        Event = @event;

        if (!string.IsNullOrEmpty(rsvp) &&
            CurrentMember != null)
        {
            try
            {
                EventResponseType response = Enum.Parse<EventResponseType>(rsvp, true);
                await EventService.UpdateMemberResponse(CurrentMember, Event.Id, response);
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
        if (CurrentMember != null)
        {
            await EventService.UpdateMemberResponse(CurrentMember, id, responseType);
        }

        return RedirectToSelf(id);
    }

    protected abstract IActionResult RedirectToSelf(Guid id);
}
