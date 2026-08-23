using Microsoft.AspNetCore.Html;

namespace ODK.Web.Razor.Models.Components;

public class BodyWithSidebarViewModel : BodyViewModel
{
    /// <summary>
    /// The breakpoint from which the sidebar sits beside the content. Below it the sidebar stacks above.
    /// </summary>
    public string SidebarBreakpoint { get; init; } = "md";

    public IHtmlContent? SidebarContent { get; set; }

    /// <summary>
    /// Which edge the sidebar sits on - "start" or "end" - once it is beside the content.
    /// </summary>
    public string SidebarPosition { get; init; } = "end";
}
