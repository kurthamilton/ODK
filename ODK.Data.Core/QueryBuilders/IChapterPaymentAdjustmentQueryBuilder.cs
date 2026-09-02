using ODK.Core.Chapters;

namespace ODK.Data.Core.QueryBuilders;

public interface IChapterPaymentAdjustmentQueryBuilder
    : IDatabaseEntityQueryBuilder<ChapterPaymentAdjustment, IChapterPaymentAdjustmentQueryBuilder>
{
    IChapterPaymentAdjustmentQueryBuilder ForChapter(Guid chapterId);

    /// <summary>
    /// Adjustments raised by the given refunds.
    /// </summary>
    IChapterPaymentAdjustmentQueryBuilder ForRefunds(IEnumerable<Guid> paymentRefundIds);

    IChapterPaymentAdjustmentQueryBuilder InCurrency(Guid currencyId);

    /// <summary>
    /// Adjustments with something left to settle, in either direction.
    /// </summary>
    IChapterPaymentAdjustmentQueryBuilder Outstanding();
}
