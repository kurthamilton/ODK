namespace ODK.Services.Emails.Parameters;

/// <summary>
/// Sent to whoever contacted a group or the site when an admin replies to their message.
/// </summary>
public sealed class ContactRequestReplyParameters : EmailTypeParameters
{
    /// <summary>
    /// Supplied under the HTML prefix: the reply is written in the rich-text editor and stored as markup.
    /// A template refers to it by the offered name, without the prefix.
    /// </summary>
    private const string ReplyName = "message.reply";

    private const string TextName = "message.text";

    public static IReadOnlyCollection<string> Names { get; } = [ReplyName, TextName];

    public required string ReplyHtml { get; init; }

    public required string Text { get; init; }

    protected override void AddParameters(IDictionary<string, string> values)
    {
        Add(values, EmailParameters.HtmlPrefix + ReplyName, ReplyHtml);
        Add(values, TextName, Text);
    }
}
