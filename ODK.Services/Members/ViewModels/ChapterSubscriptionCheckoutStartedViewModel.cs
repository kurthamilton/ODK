using ODK.Core.Payments;

namespace ODK.Services.Members.ViewModels;

public class ChapterSubscriptionCheckoutStartedViewModel
{
    public required string ClientSecret { get; init; }

    public required string CurrencyCode { get; init; }

    public required IPaymentSettings PaymentSettings { get; init; }
}
