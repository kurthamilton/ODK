namespace ODK.Services.Payments;

/// <summary>
/// What a reconcile run found to do.
/// </summary>
/// <remarks>
/// A named result rather than a <see cref="ServiceResult{T}"/> of int: two counts, and only their property
/// names can say which is which. The counts are of payments <em>queued</em>, not settled - each is read back
/// from the provider in its own job, so one the provider cannot identify fails that job rather than this call.
/// </remarks>
public class ReconcilePaymentSettlementsResult : ServiceResult
{
    private ReconcilePaymentSettlementsResult(bool success, int queued = 0, int unidentifiable = 0)
        : base(success)
    {
        Queued = queued;
        Unidentifiable = unidentifiable;
    }

    /// <summary>
    /// Payments queued to be read back from the provider.
    /// </summary>
    public int Queued { get; }

    /// <summary>
    /// Payments with no settlement that the provider cannot be asked about: they name no reference, or no
    /// payment settings, and so nothing says which account to look in. Nothing can be done for these.
    /// </summary>
    public int Unidentifiable { get; }

    public static ReconcilePaymentSettlementsResult Successful(int queued, int unidentifiable)
        => new(true, queued, unidentifiable);
}
