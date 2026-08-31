using ODK.Core.Chapters;

namespace ODK.Data.Core.QueryBuilders;

public interface IChapterPaymentAdjustmentQueryBuilder
    : IDatabaseEntityQueryBuilder<ChapterPaymentAdjustment, IChapterPaymentAdjustmentQueryBuilder>
{
    IChapterPaymentAdjustmentQueryBuilder ForChapter(Guid chapterId);

    IChapterPaymentAdjustmentQueryBuilder InCurrency(Guid currencyId);

    /// <summary>
    /// Adjustments with something left to settle, in either direction.
    /// </summary>
    IChapterPaymentAdjustmentQueryBuilder Outstanding();
}
