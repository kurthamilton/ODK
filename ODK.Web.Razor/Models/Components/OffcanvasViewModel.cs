using Microsoft.AspNetCore.Html;

namespace ODK.Web.Razor.Models.Components;

public class OffcanvasViewModel
{
    public string? BodyClass { get; init; }

    /// <summary>
    /// Only show the Offcanvas for this breakpoint size and up
    /// </summary>
    public string? Breakpoint { get; init; }

    public string? Class { get; init; }

    public IHtmlContent? Content { get; init; }

    public Func<object?, IHtmlContent>? ContentFunc { get; init; }

    public required string Id { get; init; }

    public string? Position { get; init; }

    public Func<object?, IHtmlContent>? SubtitleContent { get; init; }

    public string? Title { get; init; }

    public Func<object?, IHtmlContent>? TitleContent { get; init; }
}