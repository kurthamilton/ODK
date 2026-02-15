using ODK.Web.Common.Components;

namespace ODK.Web.Razor.Models.Components;

public class IconViewModel
{
    public IconViewModel(IconType type)
    {
        Type = type;
    }

    public string? Class { get; init; }

    public IconType Type { get; }
}