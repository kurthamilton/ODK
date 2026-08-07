namespace ODK.Web.Common.Routes;

/// <summary>
/// A single navigable group admin route paired with its menu text. Enumerating these is what lets
/// callers answer "which admin pages may this member actually see?" without hard-coding a list.
/// </summary>
public class GroupAdminNavItem
{
    public GroupAdminNavItem(GroupAdminRoute route, string text)
    {
        Route = route;
        Text = text;
    }

    public GroupAdminRoute Route { get; }

    public string Text { get; }
}
