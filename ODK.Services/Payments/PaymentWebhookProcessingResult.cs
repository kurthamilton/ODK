using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Core.Payments;

namespace ODK.Services.Payments;

internal class PaymentWebhookProcessingResult
{
    private PaymentWebhookProcessingResult()
    {
    }

    internal Chapter? Chapter { get; init; }

    internal Currency? Currency { get; init; }

    internal Member? Member { get; init; }

    internal Payment? Payment { get; init; }

    internal bool Success { get; init; }

    internal static PaymentWebhookProcessingResult Successful(
        Member? member, Chapter? chapter, Payment? payment, Currency? currency)
        => new PaymentWebhookProcessingResult
        {
            Chapter = chapter,
            Currency = currency,
            Member = member,
            Payment = payment,
            Success = true
        };

    internal static PaymentWebhookProcessingResult Failure()
        => new PaymentWebhookProcessingResult();
}