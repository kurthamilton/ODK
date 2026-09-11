namespace ODK.Web.Razor.Models.Components;

public class SideMenuViewModel
{
    public MenuItem? Active { get; init; }

    public string? Class { get; init; }

    /// <summary>
    /// Render every item's children, not just the active branch's. Sections outside the active branch
    /// render collapsed, and only the menu drawer offers a toggle to open them - see _side-menu.scss.
    /// </summary>
    public bool ExpandAll { get; init; }

    public bool Indented { get; init; } = true;

    public required IReadOnlyCollection<IReadOnlyCollection<MenuItem>> MenuItemGroups { get; init; }

    public bool Root { get; init; }
}
