using ODK.Core.Events;

namespace ODK.Services.Emails.Parameters;

/// <summary>
/// Sent to event admins, and to the member being replied to, when someone comments on an event.
/// </summary>
public sealed class EventCommentParameters : EmailTypeParameters
{
    private const string EventIdName = "event.id";

    private const string EventUrlName = "event.url";

    private const string TextName = "comment.text";

    private readonly Event _event;

    public EventCommentParameters(Event @event)
    {
        _event = @event;
    }

    public static IReadOnlyCollection<string> Names { get; } = [TextName, EventIdName, EventUrlName];

    public required string EventUrl { get; init; }

    public required string Text { get; init; }

    protected override void AddParameters(IDictionary<string, string> values)
    {
        Add(values, EventIdName, _event.Id.ToString());
        Add(values, EventUrlName, EventUrl);
        Add(values, TextName, Text);
    }
}
