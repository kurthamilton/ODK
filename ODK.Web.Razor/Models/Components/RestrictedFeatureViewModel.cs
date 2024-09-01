using Microsoft.AspNetCore.Html;
using ODK.Core.Chapters;
using ODK.Core.Features;
using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Components;

public class RestrictedFeatureViewModel
{
    public RestrictedFeatureViewModel(Chapter chapter)
    {
        Chapter = chapter;
    }

    public RestrictedFeatureViewModel(MemberSiteSubscription? ownerSubscription)
    {
        OwnerSubscription = ownerSubscription;
    }

    public Chapter? Chapter { get; }

    public IHtmlContent? Content { get; init; }

    public Func<object?, IHtmlContent>? ContentFunc { get; init; }

    public Func<object?, IHtmlContent>? DisabledContentFunc { get; init; }

    public required SiteFeatureType Feature { get; init; }

    public MemberSiteSubscription? OwnerSubscription { get; }
}
