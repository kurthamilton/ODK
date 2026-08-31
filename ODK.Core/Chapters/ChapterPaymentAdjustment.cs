namespace ODK.Core.Chapters;

/// <summary>
/// An amount owed between us and a group, which no single payment's transfer settles: what a refund's
/// reversal could not recover, and whatever a site admin raises by hand. Netted off the group's later
/// transfers until it is settled.
/// </summary>
/// <remarks>
/// A balance is per group <em>and per currency</em>. A group can take payments in more than one, and
/// amounts in different currencies cannot be netted against each other.
/// </remarks>
public class ChapterPaymentAdjustment : IDatabaseEntity, IChapterEntity
{
    /// <summary>
    /// Signed, in the currency named by <see cref="CurrencyId"/>: negative is owed to us by the group,
    /// positive is owed to the group by us.
    /// </summary>
    public decimal Amount { get; set; }

    public Guid ChapterId { get; set; }

    public DateTime CreatedUtc { get; set; }

    public Guid CurrencyId { get; set; }

    /// <summary>
    /// What the group is told this is for, which is the only account of it they get on a transfer that
    /// arrives smaller than the payment behind it.
    /// </summary>
    public required string Description { get; set; }

    public Guid Id { get; set; }

    /// <summary>
    /// The refund this arose from, where one did. Carried as a plain id: a foreign key here would be a
    /// second cascade path from Chapter into this table, which SQL Server rejects.
    /// </summary>
    public Guid? PaymentRefundId { get; set; }

    /// <summary>
    /// How much of <see cref="Amount"/> has been settled, carrying the same sign as it. Settled a piece at
    /// a time, because a debit larger than the group's next transfer takes several to recover.
    /// </summary>
    public decimal RecoveredAmount { get; set; }

    public ChapterPaymentAdjustmentType Type { get; set; }

    /// <summary>
    /// What is left to settle, signed as <see cref="Amount"/> is. Zero once the adjustment is done with.
    /// </summary>
    public decimal Outstanding() => Amount - RecoveredAmount;
}
