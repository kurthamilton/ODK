using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.Components;

namespace ODK.Web.Razor.Models.Admin;

/// <summary>
/// The group admin menu tree as menu items, plus the top-level section the current path sits under.
/// A section is navigable in its own right and holds pages that are too, so which one the current page
/// belongs to is a question about the whole tree rather than about any one link in it.
/// </summary>
public class AdminNavViewModel
{
    private AdminNavViewModel(IReadOnlyCollection<MenuItem> sections, MenuItem? activeSection)
    {
        ActiveSection = activeSection;
        Sections = sections;
    }

    public MenuItem? ActiveSection { get; }

    public IReadOnlyCollection<MenuItem> Sections { get; }

    public static AdminNavViewModel Create(IReadOnlyCollection<GroupAdminNavSection> sections, string? path)
    {
        var menuItems = sections
            .Select(section => new MenuItem
            {
                Link = section.Route.Path,
                Text = section.Text,
                Children = section.Items
                    .Select(item => new MenuItem { Link = item.Route.Path, Text = item.Text })
                    .ToArray()
            })
            .ToArray();

        var allItems = menuItems
            .SelectMany(x => x.Expand())
            .ToArray();

        var active = allItems.Active(path) ?? allItems.Closest(path);

        return new AdminNavViewModel(
            menuItems,
            active != null ? menuItems.FirstOrDefault(x => active.DescendantOf(x)) : null);
    }
}
