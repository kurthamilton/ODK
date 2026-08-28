namespace ODK.Services.Payments;

/// <summary>
/// What a payment provider says a charge actually did, read back after the event. What each party ends up
/// with is not here for a charge collected whole: with the transfer decoupled from collection, that is ours
/// to decide and record.
/// </summary>
public class ExternalPaymentSettlement
{
    /// <summary>
    /// The amount charged, in <see cref="CurrencyCode"/>.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// The charge the money arrived on, which a transfer out of it is tied to.
    /// </summary>
    public required string ChargeId { get; init; }

    /// <summary>
    /// The commission the provider collected on our behalf as part of the charge, in
    /// <see cref="CurrencyCode"/>, where it did so - which is a charge that also made its own transfer.
    /// Null for a charge collected whole, where the split is ours to make.
    /// </summary>
    public required decimal? CollectedCommissionAmount { get; init; }

    /// <summary>
    /// Whether the provider has settled the charge and said what it cost. False until it has, which is a
    /// state to wait out rather than a charge that cost nothing.
    /// </summary>
    public bool Complete => NetAmount != null;

    /// <summary>
    /// The currency the payment was presented in.
    /// </summary>
    public required string CurrencyCode { get; init; }

    /// <summary>
    /// The provider's fee, in <see cref="SettlementCurrencyCode"/>. We bear it.
    /// </summary>
    public required decimal? FeeAmount { get; init; }

    /// <summary>
    /// What the charge left in our balance, in <see cref="SettlementCurrencyCode"/>: the amount charged
    /// less the provider's fee, and before anything is transferred on.
    /// </summary>
    public required decimal? NetAmount { get; init; }

    /// <summary>
    /// The currency <see cref="FeeAmount"/> and <see cref="NetAmount"/> are in, which need not be
    /// <see cref="CurrencyCode"/>. Null alongside them.
    /// </summary>
    public required string? SettlementCurrencyCode { get; init; }

    /// <summary>
    /// When the provider made its own transfer to the connected account, where it did. Null for a charge
    /// collected whole, where making the transfer is ours to do.
    /// </summary>
    public required DateTime? TransferredUtc { get; init; }
}
