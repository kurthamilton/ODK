using Microsoft.AspNetCore.Html;

namespace ODK.Web.Razor.Models.Components;

public class DropdownViewModel
{
    public bool AlignRight { get; init; }

    public Func<object?, IHtmlContent>? ButtonContent { get; init; }

    public string? ButtonSize { get; init; }

    public IconType? ButtonIcon { get; init; }

    public string? ButtonIconClass { get; init; }

    public string? ButtonText { get; init; }

    public bool HideCaret { get; init; }

    public required IReadOnlyCollection<MenuItem> MenuItems { get; init; }
}