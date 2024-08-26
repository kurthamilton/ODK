using Microsoft.AspNetCore.Html;
using ODK.Core.Chapters;
using ODK.Core.Features;

namespace ODK.Web.Razor.Models.Components;

public class RestrictedFeatureViewModel
{
    public required Chapter Chapter { get; init; }

    public Func<object?, IHtmlContent>? ContentFunc { get; init; }

    public Func<object?, IHtmlContent>? DisabledContentFunc { get; init; }

    public required SiteFeatureType Feature { get; init; }
}
