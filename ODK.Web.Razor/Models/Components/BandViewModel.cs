using Microsoft.AspNetCore.Html;

namespace ODK.Web.Razor.Models.Components;

public class BandViewModel
{
    public string? Class { get; init; }

    public Func<object?, IHtmlContent>? Content { get; init; }

    public bool Hero { get; init; }

    public string? Id { get; init; }

    public string? Subtitle { get; init; }

    public BandThemeType? Theme { get; init; }

    public string? Title { get; init; }
}