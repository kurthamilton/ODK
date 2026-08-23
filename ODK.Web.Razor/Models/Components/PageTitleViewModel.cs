using Microsoft.AspNetCore.Html;

namespace ODK.Web.Razor.Models.Components;

public class PageTitleViewModel
{
    public string? ContainerClass { get; init; }

    public IHtmlContent? Content { get; init; }

    public string? Title { get; init; }

    public string? WidthClass { get; init; }
}
