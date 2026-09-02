using ODK.Core.Countries;
using ODK.Core.Platforms;

namespace ODK.Core.Payments;

public class Payment : IDatabaseEntity
{
    public Payment()
    {
    }

    /// <summary>
    /// What the provider actually charged, in <see cref="Currency"/>. Null until the settlement has been
    /// read back from the provider, and permanently null for a payment taken before that was recorded.
    /// </summary>
    public decimal? ActualAmount { get; set; }

    /// <summary>
    /// What we kept, in <see cref="Currency"/>: our commission, taken from <see cref="ActualNetAmount"/> so
    /// that the provider's fee comes off before we take a cut. Null for a payment taken by the site, which
    /// has no connected account to split with and keeps the net.
    /// </summary>
    public decimal? ActualCommissionAmount { get; set; }

    /// <summary>
    /// What the group's connected account is owed, in <see cref="Currency"/>:
    /// <see cref="ActualNetAmount"/> less <see cref="ActualCommissionAmount"/>. Null for a payment taken by
    /// the site. Set when the settlement is read, which is before the money moves - see
    /// <see cref="TransferredUtc"/>.
    /// </summary>
    public decimal? ActualConnectedAccountAmount { get; set; }

    /// <summary>
    /// The provider's commission, in <see cref="SettlementCurrencyCode"/>. We bear it, whether or not a
    /// connected account is involved.
    /// </summary>
    public decimal? ActualFeeAmount { get; set; }

    /// <summary>
    /// What the charge left in our balance, in <see cref="SettlementCurrencyCode"/>: the amount charged
    /// less the provider's fee, and before the group's share is transferred out of it.
    /// </summary>
    public decimal? ActualNetAmount { get; set; }

    /// <summary>
    /// What we asked for, read from our own price. Compare <see cref="ActualAmount"/>, which is what the
    /// provider says actually moved.
    /// </summary>
    public decimal Amount { get; set; }

    public Guid? ChapterId { get; set; }

    public DateTime? CreatedUtc { get; set; }

    public Currency Currency { get; set; } = null!;

    public Guid CurrencyId { get; set; }

    public EnvironmentType Environment { get; set; }

    /// <summary>
    /// The provider's charge the money arrived on. Kept because a transfer out of it names it, and
    /// because it is what takes someone from a payment row to the charge in the provider's dashboard.
    /// </summary>
    public string? ExternalChargeId { get; set; }

    public string? ExternalId { get; set; }

    /// <summary>
    /// The provider's transfer that moved <see cref="ActualConnectedAccountAmount"/> to the group. Kept
    /// because reversing a transfer names it, so a payment without it cannot be refunded from the group's
    /// share. Null for a site payment, and for a group payment not yet transferred.
    /// </summary>
    public string? ExternalTransferId { get; set; }

    public Guid Id { get; set; }

    public Guid MemberId { get; set; }

    public DateTime? PaidUtc { get; set; }

    public PaymentProviderType PaymentProvider { get; set; }

    public PlatformType Platform { get; set; }

    /// <summary>
    /// When the last reconcile gave up on the payment. Null while none has, and cleared by one that
    /// succeeds.
    /// </summary>
    public DateTime? ReconciliationFailedUtc { get; set; }

    /// <summary>
    /// What the last reconcile could not do. Recorded rather than only logged, so the reason a payment is
    /// still listed is visible beside it rather than in the error log.
    /// </summary>
    public string? ReconciliationFailureReason { get; set; }

    /// <summary>
    /// When a site admin told reconciliation to ignore the payment, because nothing the provider can be
    /// asked will ever answer for it - a charge taken through an account no longer configured, or one
    /// restored into a database whose provider keys reach a different account. An ignored payment is
    /// skipped by the job as well as hidden from the page, so a directly queued read respects it too.
    /// </summary>
    public DateTime? ReconciliationIgnoredUtc { get; set; }

    public string Reference { get; set; } = string.Empty;

    /// <summary>
    /// The currency of the balance the money settled into, which is not necessarily
    /// <see cref="Currency"/> - a provider converts when it holds no balance in the currency charged.
    /// Named rather than referenced, because a settlement currency need not be one of ours.
    /// </summary>
    public string? SettlementCurrencyCode { get; set; }

    /// <summary>
    /// The part of <see cref="ActualConnectedAccountAmount"/> that was not sent, because it was applied
    /// against what the group already owed. Null where nothing was withheld, which is every payment taken
    /// before this was recorded and every payment for a group carrying no debt.
    /// </summary>
    /// <remarks>
    /// <c>ActualConnectedAccountAmount = TransferWithheldAmount + what was transferred</c>. Written once,
    /// with <see cref="TransferredUtc"/>, and never adjusted after - so the pair states what was owed and
    /// what became of it, which one mutating figure could not. Derivable from the recovery rows and
    /// denormalised anyway, as the settlement figures are: the rows are the record, this is what a reader
    /// and a query see.
    /// </remarks>
    public decimal? TransferWithheldAmount { get; set; }

    /// <summary>
    /// When <see cref="ActualConnectedAccountAmount"/> actually reached the group. Null while it has not:
    /// a payment with a <see cref="ChapterId"/> and a settlement but no date here is money we still owe,
    /// whether the transfer is pending or has failed for good.
    /// </summary>
    public DateTime? TransferredUtc { get; set; }
}