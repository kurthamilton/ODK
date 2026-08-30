using ODK.Core.Chapters;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Services.Payments.Models;

namespace ODK.Services.Payments;

public interface IPaymentProvider
{
    /// <summary>
    /// The percentage of a payment's net we keep, the rest going to the group's connected account. Applied
    /// to the net rather than the amount charged, so the provider's own fee comes off before we take a cut.
    /// </summary>
    decimal CommissionPercentage { get; }

    /// <summary>
    /// How long after a payment to wait before <see cref="GetPaymentSettlement(string)"/> can be expected to
    /// answer. A provider settles a charge a moment after taking it, and reading sooner costs a retry.
    /// </summary>
    TimeSpan SettlementReadDelay { get; }

    PaymentProviderType Type { get; }

    Task<ServiceResult> ActivateSubscriptionPlan(string externalId);

    Task<bool> CancelSubscription(string externalId);

    Task<RemoteAccount?> CreateConnectedAccount(RemoteAccountCreateOptions options);

    Task<string?> CreateSubscriptionPlan(ExternalSubscriptionPlan subscriptionPlan);

    /// <summary>
    /// Moves money out of a charge we collected and on to a connected account. Safe to call again for the
    /// same <see cref="ExternalTransfer.IdempotencyKey"/> - the provider makes one transfer, not two.
    /// </summary>
    Task<ServiceResult> CreateTransfer(ExternalTransfer transfer);

    Task<ServiceResult> DeactivateSubscriptionPlan(string externalId);

    Task<string?> GenerateConnectedAccountSetupUrl(GenerateRemoteAccountSetupUrlOptions options);

    Task<ExternalCheckoutSession?> GetCheckoutSession(string externalId);

    Task<RemoteAccount?> GetConnectedAccount(string externalId);

    Task<string> GetOrCreateChapterProduct(Chapter chapter);

    Task<string> GetOrCreatePlatformProduct(PlatformType platform);

    /// <summary>
    /// The id of the payment that settled an invoice, for passing to
    /// <see cref="GetPaymentSettlement(string)"/>. A recurring subscription's webhook names only the invoice.
    /// </summary>
    Task<string?> GetInvoicePaymentId(string externalInvoiceId);

    /// <summary>
    /// The id of the payment behind a reference recorded when it was taken, which may name the payment
    /// itself or the subscription that billed it. Null where the reference names neither, or where it names
    /// a subscription whose invoices do not identify one payment made at <paramref name="paidUtc"/>.
    /// <para>
    /// A returned id is not an assurance that the payment exists in this account - a reference naming a
    /// payment directly is answered without asking. Only <see cref="GetPaymentSettlement(string)"/> settles
    /// that, so a caller checking which account holds a payment has to go on to read it.
    /// </para>
    /// </summary>
    Task<string?> GetPaymentIdForReference(string reference, DateTime paidUtc);

    /// <summary>
    /// What actually moved for a payment. Null where the payment or its charge cannot be read at all; a
    /// value with a null <see cref="ExternalPaymentSettlement.NetAmount"/> where the provider has yet to
    /// settle it.
    /// </summary>
    Task<ExternalPaymentSettlement?> GetPaymentSettlement(string externalPaymentId);

    string GetPublicApiKey(PlatformType platform);

    Task<ExternalSubscription?> GetSubscription(string externalId);

    Task<ExternalSubscriptionPlan?> GetSubscriptionPlan(string externalId);

    Task<ExternalCheckoutSession> StartCheckout(
        IServiceRequest request,
        string emailAddress,
        ExternalSubscriptionPlan subscriptionPlan,
        string returnPath,
        PaymentMetadataModel metadata,
        ChapterPaymentAccount? chapterPaymentAccount);
}