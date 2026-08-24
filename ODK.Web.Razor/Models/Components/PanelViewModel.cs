using Microsoft.AspNetCore.Html;

namespace ODK.Web.Razor.Models.Components;

public class PanelViewModel
{
    public IHtmlContent? BodyContent { get; init; }

    public Func<object?, IHtmlContent>? BodyContentFunc { get; init; }

    public string? Class { get; init; }

    /// <summary>
    /// The panel's title, as a heading of the level the caller wants. Takes precedence over
    /// <see cref="Title"/>, which renders through the same view model with no level of its own.
    /// </summary>
    public HeadingViewModel? Heading { get; init; }

    public string? Title { get; init; }

    public Func<object?, IHtmlContent>? TitleContentFunc { get; init; }

    public Func<object?, IHtmlContent>? TitleEndContentFunc { get; init; }
}
