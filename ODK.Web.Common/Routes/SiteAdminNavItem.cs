using System.Collections.Generic;

namespace ODK.Web.Common.Routes;

/// <summary>
/// A navigable site admin route paired with its menu text, so the site admin menu can be built
/// from the route definitions rather than duplicating them in a layout.
/// </summary>
public class SiteAdminNavItem
{
    public SiteAdminNavItem(SiteAdminRoute route, string text)
    {
        Route = route;
        Text = text;
    }

    public IReadOnlyCollection<SiteAdminNavItem> Children { get; init; } = [];

    public SiteAdminRoute Route { get; }

    public string Text { get; }
}
