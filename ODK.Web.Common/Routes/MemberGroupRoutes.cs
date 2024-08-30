using System;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Platforms;
using ODK.Core.Venues;

namespace ODK.Web.Common.Routes;

public class MemberGroupRoutes
{
    public string Event(PlatformType platform, Chapter chapter, Guid eventId)
        => $"{Events(platform, chapter)}/{eventId}";

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

    public string EventSettings(PlatformType platform, Chapter chapter)
        => $"{Events(platform, chapter)}/settings";

    public string Group(Guid id) => $"{Index()}/{id}";

    public string GroupCreate() => $"{Index()}/new";

    public string Index() => "/my/groups";

    public string Member(PlatformType platform, Chapter chapter, Guid memberId)
        => $"{Members(platform, chapter)}/{memberId}";

    public string MemberAdmin(PlatformType platform, Chapter chapter, ChapterAdminMember adminMember)
        => $"{MemberAdmins(platform, chapter)}/{adminMember.MemberId}";

    public string MemberAdmins(PlatformType platform, Chapter chapter) 
        => $"{Members(platform, chapter)}/admins";

    public string MemberEmail(PlatformType platform, Chapter chapter, Guid memberId)
        => $"{Member(platform, chapter, memberId)}/email";

    public string MemberEvents(PlatformType platform, Chapter chapter, Guid memberId)
        => $"{Member(platform, chapter, memberId)}/events";

    public string MemberImage(PlatformType platform, Chapter chapter, Guid memberId)
        => $"{Member(platform, chapter, memberId)}/image";

    public string MembersDownload(PlatformType platform, Chapter chapter)
        => $"{Members(platform, chapter)}/download";

    public string Members(PlatformType platform, Chapter chapter) => platform switch
    {
        PlatformType.DrunkenKnitwits => $"/{chapter.Name}/admin/members",
        _ => $"{Group(chapter.Id)}/members"
    };

    public string MembersEmail(PlatformType platform, Chapter chapter)
        => $"{Members(platform, chapter)}/email";

    public string MembersSubscription(PlatformType platform, Chapter chapter, ChapterSubscription subscription)
        => $"{MembersSubscriptions(platform, chapter)}/{subscription.Id}";

    public string MembersSubscriptionCreate(PlatformType platform, Chapter chapter) => platform switch
    {
        PlatformType.DrunkenKnitwits => $"{MembersSubscriptions(platform, chapter)}/Create",
        _ => $"{MembersSubscriptions(platform, chapter)}/new"
    };

    public string MembersSubscriptions(PlatformType platform, Chapter chapter)
        => $"{Members(platform, chapter)}/subscriptions";

    public string Venue(PlatformType platform, Chapter chapter, Guid venueId)
        => $"{Venues(platform, chapter)}/{venueId}";    

    public string VenueCreate(PlatformType platform, Chapter chapter) => platform switch
    {
        PlatformType.DrunkenKnitwits => $"{Venues(platform, chapter)}/Create",
        _ => $"{Venues(platform, chapter)}/new"
    };

    public string VenueEvents(PlatformType platform, Chapter chapter, Guid venueId)
        => $"{Venue(platform, chapter, venueId)}/events";

    public string Venues(PlatformType platform, Chapter chapter) => platform switch
    {
        PlatformType.DrunkenKnitwits => $"/{chapter.Name}/Admin/Events/Venues",
        _ => $"{Group(chapter.Id)}/events/venues"
    };
}
