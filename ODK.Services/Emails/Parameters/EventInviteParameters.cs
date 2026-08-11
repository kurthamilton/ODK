namespace ODK.Services.Emails.Parameters;

/// <summary>
/// Sent to members when they are invited to an event.
/// </summary>
public sealed class EventInviteParameters : EmailTypeParameters
{
    private const string DateName = "event.date";

    private const string EventIdName = "event.id";

    private const string LegacyUnsubscribeUrlName = "unsubscribeUrl";

    private const string LocationName = "event.location";

    private const string NameName = "event.name";

    private const string RsvpUrlName = "event.rsvpUrl";

    private const string TimeName = "event.time";

    private const string UnsubscribeUrlName = "account.urls.unsubscribe";

    private const string UrlName = "event.url";

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

    public string? Date { get; set; }

    public string? EventId { get; set; }

    public string? Location { get; set; }

    public string? Name { get; set; }

    public string? RsvpUrl { get; set; }

    public string? Time { get; set; }

    public string? UnsubscribeUrl { get; set; }

    public string? Url { get; set; }

    protected override void AddParameters(IDictionary<string, string> values)
    {
        Add(values, DateName, Date);
        Add(values, EventIdName, EventId);
        Add(values, LocationName, Location);
        Add(values, NameName, Name);
        Add(values, RsvpUrlName, RsvpUrl);
        Add(values, TimeName, Time);
        Add(values, UrlName, Url);
        Add(values, UnsubscribeUrlName, UnsubscribeUrl);
        Add(values, LegacyUnsubscribeUrlName, UnsubscribeUrl);
    }
}
