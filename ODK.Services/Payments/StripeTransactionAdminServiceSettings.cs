using ODK.Core.Platforms;

namespace ODK.Services.Payments;

/// <summary>
/// How to address one of a Stripe account's own objects in its dashboard.
/// </summary>
/// <remarks>
/// Every value here can legitimately be unstated, and unstated means no link - never a guessed one. See
/// <see cref="StripeWebhookAdminServiceSettings"/>, which says the same of the webhook endpoint formats.
/// </remarks>
public class StripeTransactionAdminServiceSettings
{
    public required IReadOnlyDictionary<PlatformType, string> AccountIds { get; init; }

    public required string LiveInvoiceUrlFormat { get; init; }

    public required string LivePaymentUrlFormat { get; init; }

    public required string LiveSubscriptionUrlFormat { get; init; }

    public required string TestInvoiceUrlFormat { get; init; }

    public required string TestPaymentUrlFormat { get; init; }

    public required string TestSubscriptionUrlFormat { get; init; }
}
