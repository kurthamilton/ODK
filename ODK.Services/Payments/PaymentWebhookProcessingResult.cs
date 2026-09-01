using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Core.Payments;

namespace ODK.Services.Payments;

public class PaymentWebhookProcessingResult
{
    private PaymentWebhookProcessingResult()
    {
    }

    internal Chapter? Chapter { get; init; }

    /// <summary>
    /// The checkout session the event was about, where it was about one. Carried for the same reason
    /// <see cref="Payment"/> is - a caller acts on it once the processing has committed.
    /// </summary>
    internal PaymentCheckoutSession? CheckoutSession { get; init; }

    internal Currency? Currency { get; init; }

    internal Member? Member { get; init; }

    internal Payment? Payment { get; init; }

    internal bool Success { get; init; }

    internal static PaymentWebhookProcessingResult Successful(
        Member? member,
        Chapter? chapter,
        Payment? payment,
        Currency? currency,
        PaymentCheckoutSession? checkoutSession)
        => new PaymentWebhookProcessingResult
        {
            Chapter = chapter,
            CheckoutSession = checkoutSession,
            Currency = currency,
            Member = member,
            Payment = payment,
            Success = true
        };

    internal static PaymentWebhookProcessingResult Failure()
        => new PaymentWebhookProcessingResult();
}