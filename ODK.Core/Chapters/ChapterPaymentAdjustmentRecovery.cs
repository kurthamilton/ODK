namespace ODK.Core.Chapters;

/// <summary>
/// A part of a <see cref="ChapterPaymentAdjustment"/> settled against one payment's transfer.
/// </summary>
/// <remarks>
/// This is what explains a transfer that is smaller than the payment behind it says: without it, a group
/// asking why it received less than <c>Payment.ActualConnectedAccountAmount</c> could only be answered by
/// inference from dates.
/// </remarks>
public class ChapterPaymentAdjustmentRecovery : IDatabaseEntity
{
    /// <summary>
    /// How much of the adjustment this settled, carrying the same sign as the adjustment's own amount.
    /// </summary>
    public decimal Amount { get; set; }

    public Guid ChapterPaymentAdjustmentId { get; set; }

    public DateTime CreatedUtc { get; set; }

    public Guid Id { get; set; }

    /// <summary>
    /// The payment whose transfer absorbed it. Carried as a plain id: a foreign key here would be a second
    /// cascade path from Chapter into this table, which SQL Server rejects.
    /// </summary>
    public Guid PaymentId { get; set; }
}
