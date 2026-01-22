using ODK.Core.Chapters;
using ODK.Core.Payments;
using ODK.Core.Platforms;

namespace ODK.Services.Subscriptions.ViewModels;

public class SiteSubscriptionCheckoutViewModel
{
    public required Chapter? Chapter { get; set; }

    public required string ClientSecret { get; init; }

    public required SitePaymentSettings PaymentSettings { get; init; }

    public required PlatformType Platform { get; init; }
}