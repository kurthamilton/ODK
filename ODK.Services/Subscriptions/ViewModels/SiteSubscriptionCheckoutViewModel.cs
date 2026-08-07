using ODK.Core.Chapters;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;

namespace ODK.Services.Subscriptions.ViewModels;

public class SiteSubscriptionCheckoutViewModel
{
    public required Chapter? Chapter { get; set; }

    public required string ClientSecret { get; init; }

    public required SitePaymentSettings PaymentSettings { get; init; }

    public required PlatformType Platform { get; init; }

    /// <summary>
    /// What's being bought. Stripe Elements renders no order summary, so the page has to provide the
    /// context itself.
    /// </summary>
    public required SiteSubscription SiteSubscription { get; init; }
}