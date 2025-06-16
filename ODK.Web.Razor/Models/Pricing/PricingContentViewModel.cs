using ODK.Services.Subscriptions.ViewModels;

namespace ODK.Web.Razor.Models.Pricing;

public class PricingContentViewModel
{
    public required string CheckoutUrl { get; init; }

    public required SiteSubscriptionsViewModel SiteSubscriptions { get; init; }
}
