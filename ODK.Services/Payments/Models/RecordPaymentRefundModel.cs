namespace ODK.Services.Payments.Models;

/// <summary>
/// A refund already made through the payment provider, being written down.
/// </summary>
/// <remarks>
/// Every amount is what the provider actually did, not what we intended - the whole point of recording one
/// is that it has already happened somewhere else.
/// </remarks>
public class RecordPaymentRefundModel
{
    /// <summary>
    /// What the member was given back, in the payment's currency.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// The provider's refund, where it was noted. Kept so a row can be taken back to the refund in the
    /// provider's dashboard.
    /// </summary>
    public required string? ExternalId { get; init; }

    /// <summary>
    /// The provider's reversal of the transfer, where one was made.
    /// </summary>
    public required string? ExternalReversalId { get; init; }

    /// <summary>
    /// The part of its own fee the provider gave back. Usually none, and it comes off what the group owes:
    /// the group covers what the refund cost us, and a returned fee is one less thing it cost.
    /// </summary>
    public required decimal FeeReturnedAmount { get; init; }

    /// <summary>
    /// How the site admin names the payment: the provider's charge, the provider's payment or subscription,
    /// or the reference recorded when the payment was taken.
    /// </summary>
    public required string PaymentReference { get; init; }

    public required string Reason { get; init; }

    /// <summary>
    /// What was taken back from the group's connected account by reversing the transfer. Zero where no
    /// reversal was made, which leaves the whole of the group's share outstanding.
    /// </summary>
    public required decimal ReversedAmount { get; init; }
}
