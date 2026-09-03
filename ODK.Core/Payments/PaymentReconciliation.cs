namespace ODK.Core.Payments;

/// <summary>
/// How far the reconciliation job has got with a <see cref="Payment"/>: what it could not do, and whether
/// it has been told to stop trying.
/// </summary>
/// <remarks>
/// Our own bookkeeping rather than anything the provider knows about, which is why it is not on the
/// payment. A payment has at most one, and has none at all until a reconcile has something to say - so
/// absence is the ordinary case and means nothing has gone wrong.
/// </remarks>
public class PaymentReconciliation : IDatabaseEntity
{
    /// <summary>
    /// When the last reconcile gave up on the payment. Null while none has, and cleared by one that
    /// succeeds.
    /// </summary>
    public DateTime? FailedUtc { get; set; }

    /// <summary>
    /// What the last reconcile could not do. Recorded rather than only logged, so the reason a payment is
    /// still listed is visible beside it rather than in the error log.
    /// </summary>
    public string? FailureReason { get; set; }

    public Guid Id { get; set; }

    /// <summary>
    /// When a site admin told reconciliation to ignore the payment, because nothing the provider can be
    /// asked will ever answer for it - a charge taken through an account no longer configured, or one
    /// restored into a database whose provider keys reach a different account. An ignored payment is
    /// skipped by the job as well as hidden from the page, so a directly queued read respects it too.
    /// </summary>
    public DateTime? IgnoredUtc { get; set; }

    public Guid PaymentId { get; set; }
}
