using ODK.Core.Payments;
using ODK.Core.Platforms;

namespace ODK.Services.Subscriptions.ViewModels;

public class SiteSubscriptionCheckoutViewModel
{
    public required string ClientSecret { get; init; }

    public required string CurrencyCode { get; init; }

    public required IPaymentSettings PaymentSettings { get; init; }

    public required PlatformType Platform { get; init; }
}
