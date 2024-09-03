using Microsoft.AspNetCore.Html;

namespace ODK.Web.Razor.Models.Components;

public class PanelViewModel
{
    public required Func<object?, IHtmlContent> ContentFunc { get; init; }

    public required Func<object?, IHtmlContent> TitleFunc { get; init; }
}
