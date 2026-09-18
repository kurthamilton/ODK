using ODK.Core.Events;
using ODK.Core.Venues;
using ODK.Data.Core.Events;

namespace ODK.Services.Events.ViewModels;

public class EventResponseViewModel
{
    public EventResponseViewModel(
        Event @event,
        ChapterVenue? chapterVenue,
        EventResponseType response,
        bool invited,
        EventResponseSummaryDto? responseSummary)
    {
        ChapterVenueId = chapterVenue?.Id;
        Date = @event.DateUtc;
        EndTime = @event.EndTime;
        EventId = @event.Id;
        EventName = @event.GetDisplayName();
        EventShortcode = @event.Shortcode;
        Invited = invited;
        Response = response;
        ResponseSummary = responseSummary;
        Ticketed = @event.Ticketed;
        Time = @event.Time;
        VenueName = chapterVenue?.GetName();
    }

    public Guid? ChapterVenueId { get; }

    public DateTime Date { get; }

    public TimeSpan? EndTime { get; }

    public Guid EventId { get; }

    public string EventName { get; }

    public string EventShortcode { get; }

    public bool Invited { get; }

    public bool Public { get; }

    public EventResponseType Response { get; }

    public EventResponseSummaryDto? ResponseSummary { get; }

    public bool Ticketed { get; }

    public string? Time { get; }

    public string? VenueName { get; }
}