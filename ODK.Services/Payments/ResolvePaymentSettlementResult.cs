namespace ODK.Services.Payments;

/// <summary>
/// What reconciling one payment did.
/// </summary>
/// <remarks>
/// A named result rather than a <see cref="ServiceResult{T}"/> of bool: the payload is whether money left
/// our balance, and only its property name can say that. A caller reading <c>Value</c> would have to be
/// told.
/// </remarks>
public class ResolvePaymentSettlementResult : ServiceResult
{
    private ResolvePaymentSettlementResult(bool success, string? message, bool transferred)
        : base(success, message)
    {
        Transferred = transferred;
    }

    /// <summary>
    /// Whether the group's share was sent as part of this reconcile. False for one that only read or
    /// recorded, so a caller can say which happened rather than guessing from the payment's state - where a
    /// transfer made months ago is indistinguishable from one made just now.
    /// </summary>
    public bool Transferred { get; }

    /// <summary>
    /// Nothing the provider can be asked will answer for the payment. Already recorded against it, so the
    /// reason survives the caller.
    /// </summary>
    public new static ResolvePaymentSettlementResult Failure(string message)
        => new(false, message, transferred: false);

    /// <summary>
    /// Everything outstanding was resolved.
    /// </summary>
    public static ResolvePaymentSettlementResult Resolved(bool transferred)
        => new(true, null, transferred);
}
