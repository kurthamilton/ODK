using ODK.Core.Chapters;
using ODK.Core.Payments;
using ODK.Core.Platforms;

namespace ODK.Services.Members.ViewModels;

public class ChapterSubscriptionCheckoutStartedViewModel
{
    public required Chapter Chapter { get; init; }

    public required ChapterSubscription ChapterSubscription { get; init; }

    public required string ClientSecret { get; init; }

    public required string CurrencyCode { get; init; }

    public required IPaymentSettings PaymentSettings { get; init; }

    public required PlatformType Platform { get; init; }
}
