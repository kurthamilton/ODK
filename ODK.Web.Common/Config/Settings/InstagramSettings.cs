namespace ODK.Web.Common.Config.Settings;

public class InstagramSettings
{
    public required string FeedUrl { get; init; }

    public required string FetchUserAgent { get; init; }

    public required int FetchWaitSeconds { get; init; }

    public required string PostUrl { get; init; }
}