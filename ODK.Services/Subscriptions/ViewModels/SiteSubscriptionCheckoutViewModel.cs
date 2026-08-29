using ODK.Core.Payments;
using ODK.Core.Subscriptions;

namespace ODK.Services.Subscriptions.ViewModels;

public class SiteSubscriptionCheckoutViewModel
{
    /// <summary>
    /// The provider key the checkout page hands the provider's own browser script.
    /// </summary>
    public required string ApiPublicKey { get; init; }

    public required string ClientSecret { get; init; }

    public required PaymentProviderType PaymentProvider { get; init; }

    /// <summary>
    /// What's being bought. Stripe Elements renders no order summary, so the page has to provide the
    /// context itself.
    /// </summary>
    public required SiteSubscription SiteSubscription { get; init; }
}