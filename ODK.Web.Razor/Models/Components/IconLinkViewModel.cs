using ODK.Web.Common.Components;

namespace ODK.Web.Razor.Models.Components;

public class IconLinkViewModel
{
    public required IconType Icon { get; init; }

    public required string Text { get; init; }

    public required string Url { get; init; }
}