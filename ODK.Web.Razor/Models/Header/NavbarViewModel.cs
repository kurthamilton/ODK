using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Web.Common.Components;

namespace ODK.Web.Razor.Models.Header;

public class NavbarViewModel
{    
    public string? Breakpoint { get; set; }

    public Chapter? Chapter { get; set; }

    public string? Color { get; set; }

    public bool Compact { get; set; }

    public string? CssClass { get; set; }

    public bool Fluid { get; set; }

    public bool HideAccountMenu { get; set; }

    public string? Id { get; set; }

    public Member? Member { get; set; }

    public required MenuItem[][] MenuItems { get; set; }

    public bool IsDark => Color is null or "dark";

    public bool IsLight => Color == "light";
}
