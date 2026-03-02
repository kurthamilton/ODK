using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Web.Razor.Models.Components;

namespace ODK.Web.Razor.Models.Header;

public class NavbarViewModel
{
    public string? Breakpoint { get; init; }

    public Chapter? Chapter { get; init; }

    public string? Color { get; init; }

    public string? CssClass { get; init; }

    public bool Fluid { get; init; }

    public bool HideAccountMenu { get; init; }

    public string? Id { get; init; }

    public Member? Member { get; init; }

    public required IReadOnlyCollection<MenuItemGroup> MenuItems { get; init; }

    public bool IsDark => Color is null or "dark";

    public bool IsLight => Color == "light";
}