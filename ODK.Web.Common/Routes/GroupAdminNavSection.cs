using System.Collections.Generic;

namespace ODK.Web.Common.Routes;

/// <summary>
/// A top-level group admin menu section (Group, Events, Members, ...) and the pages beneath it.
/// The section's own <see cref="Route"/> is navigable in its own right, so it participates in
/// permission filtering alongside its <see cref="Items"/>.
/// </summary>
public class GroupAdminNavSection
{
    public required IReadOnlyCollection<GroupAdminNavItem> Items { get; init; }

    /// <summary>
    /// Gated on <see cref="ODK.Core.Members.Member.SiteAdmin"/> rather than on a chapter securable.
    /// The chapter site admin pages sit inside the chapter admin tree but are not chapter-delegable,
    /// so no <see cref="ODK.Services.Security.ChapterAdminSecurable"/> can express them.
    /// </summary>
    public bool RequiresSiteAdmin { get; init; }

    public required GroupAdminRoute Route { get; init; }

    public required string Text { get; init; }
}
