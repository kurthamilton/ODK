using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class ChapterPaymentAdjustmentRecoveryQueryBuilder
    : DatabaseEntityQueryBuilder<ChapterPaymentAdjustmentRecovery, IChapterPaymentAdjustmentRecoveryQueryBuilder>,
    IChapterPaymentAdjustmentRecoveryQueryBuilder
{
    public ChapterPaymentAdjustmentRecoveryQueryBuilder(DbContext context)
        : base(context)
    {
    }

    protected override IChapterPaymentAdjustmentRecoveryQueryBuilder Builder => this;

    public IChapterPaymentAdjustmentRecoveryQueryBuilder ForAdjustments(IEnumerable<Guid> adjustmentIds)
    {
        Query = Query.Where(x => adjustmentIds.Contains(x.ChapterPaymentAdjustmentId));
        return this;
    }

    public IChapterPaymentAdjustmentRecoveryQueryBuilder ForPayments(IEnumerable<Guid> paymentIds)
    {
        Query = Query.Where(x => paymentIds.Contains(x.PaymentId));
        return this;
    }
}
