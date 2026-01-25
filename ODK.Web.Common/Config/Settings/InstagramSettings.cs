using System.Collections.Generic;

namespace ODK.Web.Common.Config.Settings;

public class InstagramSettings
{
    public required string BaseUrl { get; init; }

    public required string ChannelPath { get; init; }

    public required string FeedPath { get; init; }

    public required Dictionary<string, string> FetcherCookies { get; init; }

    public required Dictionary<string, string> FetcherHeaders { get; init; }

    public required int FetchWaitSeconds { get; init; }

    public required string PostPath { get; init; }

    public required string TagPath { get; init; }
}