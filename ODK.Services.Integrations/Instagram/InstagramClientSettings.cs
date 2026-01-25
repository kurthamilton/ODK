namespace ODK.Services.Integrations.Instagram;

public class InstagramClientSettings
{
    public required string ChannelUrl { get; init; }

    public required string ChromePath { get; init; }

    public required string FeedUrl { get; init; }

    public required IReadOnlyDictionary<string, string> Cookies { get; init; }

    public required IReadOnlyDictionary<string, string> Headers { get; init; }
}