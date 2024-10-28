using ODK.Core.Payments;

namespace ODK.Services.Members.ViewModels;

public class ChapterSubscriptionCheckoutViewModel
{
    public required string CurrencyCode { get; init; }

    public required IPaymentSettings PaymentSettings { get; init; }

    public required string SessionId { get; init; }
}
