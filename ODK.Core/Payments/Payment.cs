using ODK.Core.Countries;
using ODK.Core.Platforms;

namespace ODK.Core.Payments;

/// <summary>
/// What a member was asked to pay, and the provider's charge that answered it.
/// </summary>
/// <remarks>
/// What became of the money afterwards is not here: the group's share is a
/// <see cref="PaymentTransfer"/>, what was given back is a <see cref="PaymentRefund"/>, and how far the
/// reconciliation job has got with it is a <see cref="PaymentReconciliation"/>.
/// </remarks>
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

    public Guid Id { get; set; }

    public Guid MemberId { get; set; }

    public DateTime? PaidUtc { get; set; }

    public PaymentProviderType PaymentProvider { get; set; }

    public PlatformType Platform { get; set; }

    public string Reference { get; set; } = string.Empty;

    /// <summary>
    /// The currency of the balance the money settled into, which is not necessarily
    /// <see cref="Currency"/> - a provider converts when it holds no balance in the currency charged.
    /// Named rather than referenced, because a settlement currency need not be one of ours.
    /// </summary>
    public string? SettlementCurrencyCode { get; set; }

    /// <summary>
    /// What is still to be given back: what the charge actually took, less what the payment's live refunds
    /// have already claimed. Null where the settlement has never been read, so nothing says what the
    /// charge took and there is nothing to measure a refund against.
    /// </summary>
    /// <remarks>
    /// How much is left, not whether a refund can be made - refunding through the provider also needs a
    /// charge to refund against, which a payment settled before charge ids were recorded does not name.
    /// A refund of one of those can still be written down.
    /// </remarks>
    /// <param name="refunds">
    /// Every refund of this payment. Which of them count is this method's to decide, so a caller cannot
    /// measure against a different set than the next one does.
    /// </param>
    public decimal? RefundableAmount(IEnumerable<PaymentRefund> refunds)
        => ActualAmount != null
            ? ActualAmount.Value - refunds.Where(x => x.IsLive).Sum(x => x.ActualAmount ?? x.Amount)
            : null;
}
