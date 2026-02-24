using Microsoft.AspNetCore.Html;
using ODK.Core.Chapters;
using ODK.Core.Features;
using ODK.Core.Subscriptions;

namespace ODK.Web.Razor.Models.Components;

public class RestrictedFeatureViewModel
{
    public RestrictedFeatureViewModel()
    {
    }

    public RestrictedFeatureViewModel(SiteSubscription? ownerSubscription)
    {
        OwnerSubscription = ownerSubscription;
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

    public SiteSubscription? OwnerSubscription { get; }

    public bool? Permitted { get; }
}
