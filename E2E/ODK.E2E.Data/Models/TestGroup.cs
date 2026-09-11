namespace ODK.E2E.Data.Models;

/// <summary>
/// A provisioned test group (Chapter). <see cref="Slug"/> is what a group's URLs are keyed by, both
/// member-facing (<c>/groups/{slug}/join</c>) and admin (<c>/my/groups/{slug}</c>);
/// <see cref="ChapterId"/> drives the DB paths and the admin controller endpoints a form posts to.
/// </summary>
public sealed record TestGroup(Guid ChapterId, string Slug, string Name);