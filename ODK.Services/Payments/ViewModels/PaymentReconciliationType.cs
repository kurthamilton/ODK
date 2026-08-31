namespace ODK.Services.Payments.ViewModels;

/// <summary>
/// What reconciling a payment will actually do. Named for the effect rather than for what the payment is
/// missing, because the two do not line up: the payment missing its <em>transfer id</em> is the one where
/// no money moves, since the money already moved.
/// </summary>
public enum PaymentReconciliationType
{
    None,

    /// <summary>
    /// Reads what the charge settled for. Writes to our own records and nothing else - the payment has no
    /// connected account to send anything to.
    /// </summary>
    Settlement,

    /// <summary>
    /// Reads what the charge settled for and then sends the group its share. The only kind that moves
    /// money.
    /// </summary>
    SettlementAndTransfer,

    /// <summary>
    /// Records the transfer that already sent the group its share, so a refund has something to reverse.
    /// Writes to our own records and nothing else.
    /// </summary>
    TransferRecord
}
