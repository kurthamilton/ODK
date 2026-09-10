namespace ODK.Services.Emails.Parameters;

/// <summary>
/// Carries a message in a conversation, whether it is with a group or with the site, and whether it is
/// read by the member or by an admin. Only the template differs between the four.
/// </summary>
/// <remarks>
/// <see cref="Subject"/> carries the "Re: " marker on a reply rather than the template doing so, since a
/// stored subject cannot vary. The default templates open with it, so a reply reads exactly as it did
/// when the marker was prefixed to the whole line.
/// </remarks>
public sealed class ConversationParameters : EmailTypeParameters
{
    private const string MessageName = "conversation.message";

    private const string SubjectName = "conversation.subject";

    private const string UrlName = "conversation.url";

    public static IReadOnlyCollection<string> Names { get; } = [MessageName, SubjectName, UrlName];

    public required string Message { get; init; }

    public required string Subject { get; init; }

    public required string Url { get; init; }

    protected override void AddParameters(IDictionary<string, string> values)
    {
        Add(values, MessageName, Message);
        Add(values, SubjectName, Subject);
        Add(values, UrlName, Url);
    }
}
