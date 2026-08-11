namespace ODK.Services.Emails.Parameters;

/// <summary>
/// Sent to event admins, and to the member being replied to, when someone comments on an event.
/// </summary>
public sealed class EventCommentParameters : EmailTypeParameters
{
    private const string EventIdName = "event.id";

    private const string EventUrlName = "event.url";

    private const string TextName = "comment.text";

    public static IReadOnlyCollection<string> Names { get; } = [TextName, EventIdName, EventUrlName];

    public string? EventId { get; set; }

    public string? EventUrl { get; set; }

    public string? Text { get; set; }

    protected override void AddParameters(IDictionary<string, string> values)
    {
        Add(values, TextName, Text);
        Add(values, EventIdName, EventId);
        Add(values, EventUrlName, EventUrl);
    }
}
