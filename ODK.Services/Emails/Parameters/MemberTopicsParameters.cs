using ODK.Core.Topics;

namespace ODK.Services.Emails.Parameters;

/// <summary>
/// Tells a member what has become of the topics they suggested.
/// </summary>
/// <remarks>
/// One class across approval and rejection. They are sent the same values from the same shape of method;
/// only the template differs, which is the part an admin edits.
/// </remarks>
public sealed class MemberTopicsParameters : EmailTypeParameters
{
    /// <summary>
    /// Supplied under the HTML prefix, which is how EmailService knows to interpolate it without
    /// encoding. A template refers to it by the offered name, without the prefix.
    /// </summary>
    private const string TopicsName = "topics";

    private readonly IReadOnlyCollection<INewTopic> _topics;

    public MemberTopicsParameters(IReadOnlyCollection<INewTopic> topics)
    {
        _topics = topics;
    }

    public static IReadOnlyCollection<string> Names { get; } = [TopicsName];

    protected override void AddParameters(IDictionary<string, string> values)
    {
        var topicsHtml = new EmailTableBuilder()
            .AddRows(_topics, x => new[] { x.TopicGroup, x.Topic })
            .ToString();

        Add(values, EmailParameters.HtmlPrefix + TopicsName, topicsHtml);
    }
}
