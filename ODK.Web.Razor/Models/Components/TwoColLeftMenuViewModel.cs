using Microsoft.AspNetCore.Html;

namespace ODK.Web.Razor.Models.Components;

public class TwoColLeftMenuViewModel
{
    public required Func<object?, IHtmlContent> BodyContentFunc { get; init; }

    public BreadcrumbsViewModel? Breadcrumbs { get; init; }

    public required Func<object?, Task<IHtmlContent>> MenuContentFunc { get; init; }

    /// <summary>
    /// Titles the menu drawer the menu collapses into below the breakpoint
    /// </summary>
    public required string MenuTitle { get; init; }

    public string? Title { get; init; }
}
