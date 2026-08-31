namespace ODK.Core.Payments;

/// <summary>
/// A refund of some or all of a <see cref="Payment"/>: what the member is given back, and what was
/// recovered from the group to cover it.
/// </summary>
/// <remarks>
/// A payment has many, because a refund can be partial and can be retried after a failure. The sum of a
/// payment's refunds cannot exceed its <see cref="Payment.ActualAmount"/>, and the sum of their
/// <see cref="ReversedAmount"/> cannot exceed its <see cref="Payment.ActualConnectedAccountAmount"/>.
/// </remarks>
public class PaymentRefund : IDatabaseEntity
{
    /// <summary>
    /// What the provider says actually left our balance, in the payment's currency. Null until the refund
    /// has been read back from the provider.
    /// </summary>
    public decimal? ActualAmount { get; set; }

    /// <summary>
    /// What the member is to be given back, in the payment's currency. What we asked for; compare
    /// <see cref="ActualAmount"/>, which is what the provider says moved.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// How much of <see cref="Amount"/> the group is liable for, in the payment's currency. Null for a
    /// site payment, which has no group to recover from. Recorded per refund rather than derived, so what
    /// the group was asked to bear stays what it was asked to bear when the policy behind it changes.
    /// </summary>
    public decimal? ChapterAmount { get; set; }

    /// <summary>
    /// Why the refund was refused. Set with <see cref="PaymentRefundStatusType.Declined"/>, and written
    /// for the member to read.
    /// </summary>
    public string? DeclinedReason { get; set; }

    /// <summary>
    /// The provider's refund. Set as soon as the provider accepts it, which is before it confirms what it
    /// did - so this identifies a refund whose outcome is still unknown.
    /// </summary>
    public string? ExternalId { get; set; }

    /// <summary>
    /// The provider's reversal of the transfer, where the group's share was recovered by reversing it.
    /// </summary>
    public string? ExternalReversalId { get; set; }

    /// <summary>
    /// Why the provider failed the refund after taking it. Set with
    /// <see cref="PaymentRefundStatusType.Failed"/>.
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// The part of the provider's own fee it gave back, in <see cref="SettlementCurrencyCode"/>. Read from
    /// the refund rather than assumed: whether a provider returns its fee varies by account and country,
    /// and it is what the group has to cover that turns on the answer.
    /// </summary>
    public decimal? FeeReturnedAmount { get; set; }

    public Guid Id { get; set; }

    public Guid PaymentId { get; set; }

    /// <summary>
    /// Why the refund was asked for. Stated rather than optional: it is the whole of the audit trail a
    /// group has for money leaving its account.
    /// </summary>
    public required string Reason { get; set; }

    /// <summary>
    /// When the provider confirmed the money left us. Null while it has not: a refund with an
    /// <see cref="ExternalId"/> and no date here is one whose outcome we have yet to read.
    /// </summary>
    public DateTime? RefundedUtc { get; set; }

    /// <summary>
    /// Who asked. A member requesting their own refund, or the admin raising it on their behalf.
    /// </summary>
    public Guid RequestedByMemberId { get; set; }

    public DateTime RequestedUtc { get; set; }

    /// <summary>
    /// Who approved or declined it. Null while it is still <see cref="PaymentRefundStatusType.Requested"/>.
    /// </summary>
    public Guid? ResolvedByMemberId { get; set; }

    /// <inheritdoc cref="ResolvedByMemberId"/>
    public DateTime? ResolvedUtc { get; set; }

    /// <summary>
    /// What came back from the group's connected account by reversing the transfer, in the payment's
    /// currency. Less than <see cref="ChapterAmount"/> where the transfer could not cover it - the
    /// remainder is a <c>ChapterPaymentAdjustment</c>, recovered from the group's later transfers.
    /// </summary>
    public decimal? ReversedAmount { get; set; }

    public DateTime? ReversedUtc { get; set; }

    /// <summary>
    /// The currency <see cref="FeeReturnedAmount"/> is in, which need not be the payment's own - a
    /// provider converts when it holds no balance in the currency charged. Null alongside it.
    /// </summary>
    public string? SettlementCurrencyCode { get; set; }

    public PaymentRefundStatusType Status { get; set; }
}
