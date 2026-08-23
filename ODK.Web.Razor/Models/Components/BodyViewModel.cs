using Microsoft.AspNetCore.Html;

namespace ODK.Web.Razor.Models.Components;

public class BodyViewModel
{
    public const string DefaultContainerClass = "container-md";

    public IReadOnlyCollection<MenuItem>? Breadcrumbs { get; init; }

    public string? Class { get; init; }

    public string? ContainerClass { get; init; }

    public IHtmlContent? Content { get; init; }

    public Func<object?, IHtmlContent>? ContentFunc { get; init; }

    public bool HideSubscriptionAlert { get; init; }

    public IHtmlContent? Menu { get; init; }

    public string? Title { get; init; }

    public IHtmlContent? TitleContent { get; init; }

    public string? WidthClass { get; init; }
}
