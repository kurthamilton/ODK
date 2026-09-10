using ODK.Core.Topics;

namespace ODK.Services.Emails.Parameters;

/// <summary>
/// Tells site admins that members have suggested topics that need approving.
/// </summary>
public sealed class NewTopicAdminParameters : EmailTypeParameters
{
    /// <summary>
    /// Supplied under the HTML prefix, which is how EmailService knows to interpolate it without
    /// encoding. A template refers to it by the offered name, without the prefix.
    /// </summary>
    private const string TopicsName = "topics";

    private const string UrlName = "siteadmin.urls.topics";

    private readonly IReadOnlyCollection<INewTopic> _topics;

    public NewTopicAdminParameters(IReadOnlyCollection<INewTopic> topics)
    {
        _topics = topics;
    }

    public static IReadOnlyCollection<string> Names { get; } = [TopicsName, UrlName];

    public required string Url { get; init; }

    protected override void AddParameters(IDictionary<string, string> values)
    {
        var topicsHtml = new EmailTableBuilder()
            .AddRows(_topics, x => new[] { x.TopicGroup, x.Topic })
            .ToString();

        Add(values, EmailParameters.HtmlPrefix + TopicsName, topicsHtml);
        Add(values, UrlName, Url);
    }
}
