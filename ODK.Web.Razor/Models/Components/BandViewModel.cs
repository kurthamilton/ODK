using Microsoft.AspNetCore.Html;

namespace ODK.Web.Razor.Models.Components;

public class BandViewModel
{
    public string? Class { get; init; }

    public Func<object?, IHtmlContent>? Content { get; init; }

    /// <summary>
    /// The band's title. A band is a division of a marketing page rather than of its content, so the level
    /// belongs to the page that lays the bands out: the first band holds the page's H1 and the rest sit
    /// under it.
    /// </summary>
    public HeadingViewModel? Heading { get; init; }

    public bool Hero { get; init; }

    public string? Id { get; init; }

    public string? Subtitle { get; init; }

    public BandThemeType? Theme { get; init; }
}
