using ODK.Core.Chapters;

namespace ODK.Data.Core.QueryBuilders;

public interface IChapterPaymentAdjustmentRecoveryQueryBuilder
    : IDatabaseEntityQueryBuilder<ChapterPaymentAdjustmentRecovery, IChapterPaymentAdjustmentRecoveryQueryBuilder>
{
    IChapterPaymentAdjustmentRecoveryQueryBuilder ForAdjustments(IEnumerable<Guid> adjustmentIds);

    IChapterPaymentAdjustmentRecoveryQueryBuilder ForPayments(IEnumerable<Guid> paymentIds);
}
