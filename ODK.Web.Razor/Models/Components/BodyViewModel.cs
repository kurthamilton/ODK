using Microsoft.AspNetCore.Html;
using ODK.Web.Common.Components;

namespace ODK.Web.Razor.Models.Components;

public class BodyViewModel
{
    public IReadOnlyCollection<MenuItem>? Breadcrumbs { get; set; }

    public IHtmlContent? Content { get; set; }

    public Func<object?, IHtmlContent>? ContentFunc { get; set; }

    public bool HideSubscriptionAlert { get; set; }

    public IHtmlContent? Menu { get; set; }

    public bool Narrow { get; set; }

    public string? Title { get; set; }

    public IHtmlContent? TitleContent { get; set; }
}
