namespace ODK.Services.Emails.Parameters;

/// <summary>
/// Sent to admins when someone contacts a group, and to site admins when someone contacts the site.
/// </summary>
public sealed class ContactRequestParameters : EmailTypeParameters
{
    private const string FromName = "message.from";

    private const string TextName = "message.text";

    private const string UrlName = "message.url";

    public static IReadOnlyCollection<string> Names { get; } = [FromName, TextName, UrlName];

    public required string From { get; init; }

    public required string Text { get; init; }

    public required string Url { get; init; }

    protected override void AddParameters(IDictionary<string, string> values)
    {
        Add(values, FromName, From);
        Add(values, TextName, Text);
        Add(values, UrlName, Url);
    }
}
