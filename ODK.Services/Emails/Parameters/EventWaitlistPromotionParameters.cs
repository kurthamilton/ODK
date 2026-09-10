using System.Globalization;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Extensions;

namespace ODK.Services.Emails.Parameters;

/// <summary>
/// Tells a member on an event's waiting list that a place has opened up.
/// </summary>
public sealed class EventWaitlistPromotionParameters : EmailTypeParameters
{
    private const string DateName = "event.date";

    private const string NameName = "event.name";

    private const string UrlName = "event.url";

    private readonly Chapter _chapter;
    private readonly CultureInfo _culture;
    private readonly Event _event;

    public EventWaitlistPromotionParameters(Chapter chapter, Event @event, CultureInfo culture)
    {
        _chapter = chapter;
        _culture = culture;
        _event = @event;
    }

    public static IReadOnlyCollection<string> Names { get; } = [DateName, NameName, UrlName];

    public required string Url { get; init; }

    protected override void AddParameters(IDictionary<string, string> values)
    {
        // The venue's date, as every event time is - see EventInviteParameters.
        Add(values, DateName, _chapter.ToLocalTime(_event.DateUtc).ToString("dddd dd MMMM, yyyy", _culture));
        Add(values, NameName, _event.GetDisplayName());
        Add(values, UrlName, Url);
    }
}
