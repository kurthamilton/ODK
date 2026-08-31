namespace ODK.Core.Chapters;

/// <summary>
/// What raised a <see cref="ChapterPaymentAdjustment"/>. Numbered explicitly: the value is persisted, so
/// renumbering would reinterpret every adjustment already recorded.
/// </summary>
public enum ChapterPaymentAdjustmentType
{
    None = 0,

    /// <summary>
    /// The part of a refund the transfer reversal could not recover, because the group's share of the
    /// payment was smaller than the refund.
    /// </summary>
    RefundShortfall = 1,

    /// <summary>
    /// Raised by a site admin, in either direction, for anything the other members do not describe.
    /// </summary>
    Manual = 2,

    /// <summary>
    /// Cancels an outstanding balance we have decided not to pursue. A credit, so it settles a debit
    /// through the same netting as everything else rather than by deleting it.
    /// </summary>
    WriteOff = 3
}
