using System.Globalization;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Extensions;
using ODK.Core.Venues;

namespace ODK.Services.Emails.Parameters;

/// <summary>
/// Sent to members when they are invited to an event.
/// </summary>
public sealed class EventInviteParameters : EmailTypeParameters
{
    private const string DateName = "event.date";

    private const string EventIdName = "event.id";

    private const string LocationName = "event.location";

    private const string NameName = "event.name";

    private const string RsvpUrlName = "event.rsvpUrl";

    private const string TimeName = "event.time";

    private const string UnsubscribeUrlName = "account.urls.unsubscribe";

    private const string UrlName = "event.url";

    private readonly Chapter _chapter;
    private readonly CultureInfo _culture;
    private readonly Event _event;
    private readonly Venue _venue;

    public EventInviteParameters(Chapter chapter, Event @event, Venue venue, CultureInfo culture)
    {
        _chapter = chapter;
        _culture = culture;
        _event = @event;
        _venue = venue;
    }

    public static IReadOnlyCollection<string> Names { get; } =
    [
        DateName,
        EventIdName,
        LocationName,
        NameName,
        RsvpUrlName,
        TimeName,
        UrlName,
        UnsubscribeUrlName
    ];

    public required string RsvpUrl { get; init; }

    public required string UnsubscribeUrl { get; init; }

    public required string Url { get; init; }

    protected override void AddParameters(IDictionary<string, string> values)
    {
        Add(values, DateName, _chapter.ToLocalTime(_event.DateUtc).ToString("dddd dd MMMM, yyyy", _culture));
        Add(values, EventIdName, _event.Id.ToString());
        Add(values, LocationName, _venue.Name);
        Add(values, NameName, _event.GetDisplayName());
        Add(values, RsvpUrlName, RsvpUrl);
        Add(values, TimeName, _event.ToLocalTimeString(_chapter.TimeZone));
        Add(values, UrlName, Url);
        Add(values, UnsubscribeUrlName, UnsubscribeUrl);
    }
}
