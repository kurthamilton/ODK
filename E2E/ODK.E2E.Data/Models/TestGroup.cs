namespace ODK.E2E.Data.Models;

/// <summary>
/// A provisioned test group (Chapter). <see cref="Slug"/> is needed to drive the member-facing join
/// URL (<c>/groups/{slug}/join</c>); <see cref="ChapterId"/> drives the admin/DB paths.
/// </summary>
public sealed record TestGroup(Guid ChapterId, string Slug, string Name);