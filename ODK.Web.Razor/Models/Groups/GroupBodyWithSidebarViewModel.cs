using Microsoft.AspNetCore.Html;

namespace ODK.Web.Razor.Models.Groups;

public class GroupBodyWithSidebarViewModel : GroupBodyViewModel
{
    public required IHtmlContent SidebarContent { get; init; }
}
