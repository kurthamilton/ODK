using ODK.Web.Common.Components;

namespace ODK.Web.Razor.Models.Components;

public class SideMenuViewModel
{
    public MenuItem? Active { get; init; }

    public required IReadOnlyCollection<MenuItem> MenuItems { get; init; }

    public bool Root { get; init; }
}
