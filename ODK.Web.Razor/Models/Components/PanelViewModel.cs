using Microsoft.AspNetCore.Html;

namespace ODK.Web.Razor.Models.Components;

public class PanelViewModel
{
    public IHtmlContent? BodyContent { get; init; }

    public Func<object?, IHtmlContent>? BodyContentFunc { get; init; }

    public string? Class { get; init; }

    /// <summary>
    /// The panel's title, as a heading of the level the caller wants.
    /// </summary>
    public HeadingViewModel? Heading { get; init; }

    public Func<object?, IHtmlContent>? TitleEndContentFunc { get; init; }
}
