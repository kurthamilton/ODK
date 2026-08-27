using Microsoft.AspNetCore.Html;

namespace ODK.Web.Razor.Models.Components;

/// <summary>
/// A division of a page's own content, introduced by its own heading. The default; a
/// <see cref="PanelViewModel"/> is the opt-in raised look, for a self-contained group that needs visual
/// separation from what surrounds it.
/// </summary>
/// <remarks>
/// Deliberately the same surface as <see cref="PanelViewModel"/>, so promoting a section to a panel is a
/// one-word change at the call site rather than a rewrite.
/// </remarks>
public class SectionViewModel
{
    public IHtmlContent? BodyContent { get; init; }

    public Func<object?, IHtmlContent>? BodyContentFunc { get; init; }

    public string? Class { get; init; }

    /// <summary>
    /// The section's title, as a heading of the level the caller wants.
    /// </summary>
    public required HeadingViewModel? Heading { get; init; }

    public string? Id { get; init; }

    public Func<object?, IHtmlContent>? TitleEndContentFunc { get; init; }
}
