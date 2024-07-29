using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Venues;
using ODK.Services.Events;

namespace ODK.Web.Razor.Models.Events;

public class EventContentViewModel
{
    public EventContentViewModel(
        Member? currentMember,
        Chapter chapter, 
        Event @event, 
        Venue venue, 
        EventCommentsDto? comments)
    {
        Chapter = chapter;
        Comments = comments;
        CurrentMember = currentMember;
        Event = @event;
        Venue = venue;
    }

    public Chapter Chapter { get; }

    public EventCommentsDto? Comments { get; }

    public Member? CurrentMember { get; }

    public Event Event { get; }    

    public Venue Venue { get; }
}
