namespace ODK.Web.Razor.Models.Components;

public class SideMenuViewModel
{
    public MenuItem? Active { get; init; }

    /// <summary>
    /// Render every item's children, not just the active branch's. Sections outside the active branch
    /// render collapsed, and only the menu drawer offers a toggle to open them - see _side-menu.scss.
    /// </summary>
    public bool ExpandAll { get; init; }

    public required IReadOnlyCollection<MenuItem> MenuItems { get; init; }

    public bool Root { get; init; }
}
