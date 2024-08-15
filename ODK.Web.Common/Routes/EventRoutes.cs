using System;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Platforms;

namespace ODK.Web.Common.Routes;

public class EventRoutes
{
    public string Event(PlatformType platform, Chapter chapter, Guid eventId) => $"{BaseUrl(platform, chapter)}/{@eventId}";

    private string BaseUrl(PlatformType platform, Chapter chapter) => platform switch
    {
        PlatformType.DrunkenKnitwits => $"/{chapter.Name}/Events",
        _ => $"/groups/{chapter.Slug.ToLowerInvariant()}/events"
    };
}
