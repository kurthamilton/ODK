using ODK.Web.Common.Components;
using ODK.Web.Common.Routes;

namespace ODK.Web.Razor.Models.Admin;

public class AdminLinkViewModel
{
    public string? Class { get; init; }

    public IconType? Icon { get; init; }

    public required GroupAdminRoute Route { get; init; }

    public required string Text { get; init; }
}
