using Microsoft.AspNetCore.Html;
using ODK.Core.Chapters;
using ODK.Core.Features;

namespace ODK.Web.Razor.Models.Components;

public class RestrictedFeatureViewModel
{
    public RestrictedFeatureViewModel(IEnumerable<SiteFeatureType> ownerSubscriptionFeatures)
    {
        OwnerSubscriptionFeatures = ownerSubscriptionFeatures.ToArray();
    }

    public RestrictedFeatureViewModel(bool permitted)
    {
        Permitted = permitted;
    }

    public required Chapter? Chapter { get; init; }

    public IHtmlContent? Content { get; init; }

    public Func<object?, IHtmlContent>? ContentFunc { get; init; }

    public Func<object?, IHtmlContent>? DisabledContentFunc { get; init; }

    public required SiteFeatureType Feature { get; init; }

    public IReadOnlyCollection<SiteFeatureType> OwnerSubscriptionFeatures { get; } = [];

    public bool? Permitted { get; }
}