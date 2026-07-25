namespace ODK.E2E.Data.Models;

/// <summary>
/// A provisioned test event. <see cref="Shortcode"/> drives the member-facing event/RSVP URLs
/// (<c>/.../events/{shortcode}</c>); <see cref="EventId"/> drives the DB assertions and the
/// event-page RSVP POST (<c>/events/{eventId}/rsvp</c>).
/// </summary>
public sealed record TestEvent(Guid EventId, string Name, string Shortcode);
