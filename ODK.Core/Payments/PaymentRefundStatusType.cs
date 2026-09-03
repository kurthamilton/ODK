namespace ODK.Core.Payments;

/// <summary>
/// Where a refund has got to, mirroring the provider's own view of it. Numbered explicitly: the value is
/// persisted, so renumbering would reinterpret every refund already recorded.
/// </summary>
public enum PaymentRefundStatusType
{
    None = 0,

    /// <summary>
    /// Submitted to the provider, which has not yet confirmed what it did.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// The provider confirmed the money left us. Terminal.
    /// </summary>
    Refunded = 2,

    /// <summary>
    /// The provider took the refund and then failed it, returning the money to our balance. Terminal, and
    /// a state a member has to be told about: they have not been paid.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Withdrawn before the money moved. Terminal.
    /// </summary>
    Cancelled = 4
}
