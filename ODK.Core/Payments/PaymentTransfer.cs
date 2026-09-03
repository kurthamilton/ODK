namespace ODK.Core.Payments;

/// <summary>
/// The group's share of a <see cref="Payment"/>: what the connected account is owed, and what became of
/// it.
/// </summary>
/// <remarks>
/// Written in two steps, because the two happen at different times. Reading the settlement works out what
/// the group is owed and records the row; the transfer job then moves the money and sets
/// <see cref="CompletedUtc"/>. A payment taken by the site has none - there is no connected account to
/// split with, so the site keeps the net.
/// </remarks>
public class PaymentTransfer : IDatabaseEntity
{
    /// <summary>
    /// What the group's connected account is owed, in the payment's settlement currency: the payment's
    /// net less <see cref="CommissionAmount"/>.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// What we kept, in the payment's settlement currency: our commission, taken from the payment's net so
    /// that the provider's fee comes off before we take a cut.
    /// </summary>
    public decimal CommissionAmount { get; set; }

    /// <summary>
    /// When <see cref="Amount"/> was discharged - either sent or withheld against a debt. Null while it
    /// has not been: a transfer with no date here is money we still owe, whether the attempt is pending or
    /// has failed for good.
    /// </summary>
    public DateTime? CompletedUtc { get; set; }

    /// <summary>
    /// When the group's share was worked out, which is when the payment's settlement was read.
    /// </summary>
    public DateTime CreatedUtc { get; set; }

    /// <summary>
    /// The provider's transfer that moved the money. Kept because reversing a transfer names it, so a
    /// payment without it cannot be refunded from the group's share. Null where the provider was never
    /// asked - the whole share was withheld - and for a transfer made before ids were recorded, which is
    /// the pair <see cref="CompletedUtc"/> set with neither this nor <see cref="WithheldAmount"/>.
    /// </summary>
    public string? ExternalId { get; set; }

    public Guid Id { get; set; }

    public Guid PaymentId { get; set; }

    /// <summary>
    /// The part of <see cref="Amount"/> that was not sent, because it was applied against what the group
    /// already owed. Null where nothing was withheld, which is every transfer for a group carrying no
    /// debt.
    /// </summary>
    /// <remarks>
    /// <c>Amount = WithheldAmount + what was transferred</c>. Written once, with
    /// <see cref="CompletedUtc"/>, and never adjusted after - so the pair states what was owed and what
    /// became of it, which one mutating figure could not. Derivable from the recovery rows and
    /// denormalised anyway: the rows are the record, this is what a reader and a query see.
    /// </remarks>
    public decimal? WithheldAmount { get; set; }

    /// <summary>
    /// What can still be taken back off the group: what was actually sent, less what earlier reversals
    /// have already recovered. Zero once the transfer has given back everything it carried, and for one
    /// whose whole share was withheld - what never reached the group cannot come back from it.
    /// </summary>
    public decimal ReversibleAmount(IEnumerable<PaymentTransferReversal> reversals)
        => Amount - (WithheldAmount ?? 0) - reversals.Sum(x => x.ActualAmount ?? x.Amount);
}
