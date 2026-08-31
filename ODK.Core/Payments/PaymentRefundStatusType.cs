namespace ODK.Core.Payments;

/// <summary>
/// Where a refund has got to. Numbered explicitly: the value is persisted, so renumbering would
/// reinterpret every refund already recorded.
/// </summary>
public enum PaymentRefundStatusType
{
    None = 0,

    /// <summary>
    /// Raised and awaiting a decision. Nothing has moved.
    /// </summary>
    Requested = 1,

    /// <summary>
    /// Refused. Terminal, and the reason is the member's to be told.
    /// </summary>
    Declined = 2,

    /// <summary>
    /// Agreed to, and not yet submitted to the provider.
    /// </summary>
    Approved = 3,

    /// <summary>
    /// Submitted to the provider, which has not yet confirmed what it did.
    /// </summary>
    Refunding = 4,

    /// <summary>
    /// The provider confirmed the money left us. Terminal.
    /// </summary>
    Refunded = 5,

    /// <summary>
    /// The provider took the refund and then failed it, returning the money to our balance. Terminal, and
    /// a state a member has to be told about: they have not been paid.
    /// </summary>
    Failed = 6
}
