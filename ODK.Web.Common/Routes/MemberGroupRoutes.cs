using System;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Platforms;
using ODK.Core.Venues;

namespace ODK.Web.Common.Routes;

public class MemberGroupRoutes
{
    public string Event(PlatformType platform, Chapter chapter, Event @event)
        => $"{Events(platform, chapter)}/{@event.Id}";

    public string EventCreate(PlatformType platform, Chapter chapter) => platform switch
    {
        PlatformType.DrunkenKnitwits => $"{Events(platform, chapter)}/create",
        _ => $"{Events(platform, chapter)}/new"
    };

    public string EventInvites(PlatformType platform, Chapter chapter, Event @event)
        => $"{Events(platform, chapter)}/{@event.Id}/invites";

    public string Events(PlatformType platform, Chapter chapter) => platform switch
    {
        PlatformType.DrunkenKnitwits => $"/{chapter.Name}/Admin/Events",
        _ => $"{Group(chapter.Id)}/events"
    };

    public string Group(Guid id) => $"{Index()}/{id}";

    public string GroupCreate() => $"{Index()}/new";

    public string Index() => "/my/groups";

    public string Venue(PlatformType platform, Chapter chapter, Venue venue)
        => $"{Venues(platform, chapter)}/{venue.Id}";

    public string VenueCreate(PlatformType platform, Chapter chapter) => platform switch
    {
        PlatformType.DrunkenKnitwits => $"/{Venues(platform, chapter)}/Create",
        _ => $"/{Venues(platform, chapter)}/new"
    };

    public string Venues(PlatformType platform, Chapter chapter) => platform switch
    {
        PlatformType.DrunkenKnitwits => $"/{chapter.Name}/Admin/Events/Venues",
        _ => $"{Group(chapter.Id)}/venues"
    };
}
