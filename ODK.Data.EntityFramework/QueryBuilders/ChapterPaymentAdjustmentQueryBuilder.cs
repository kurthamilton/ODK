using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class ChapterPaymentAdjustmentQueryBuilder
    : DatabaseEntityQueryBuilder<ChapterPaymentAdjustment, IChapterPaymentAdjustmentQueryBuilder>,
    IChapterPaymentAdjustmentQueryBuilder
{
    public ChapterPaymentAdjustmentQueryBuilder(DbContext context)
        : base(context)
    {
    }

    protected override IChapterPaymentAdjustmentQueryBuilder Builder => this;

    public IChapterPaymentAdjustmentQueryBuilder ForChapter(Guid chapterId)
    {
        Query = Query.Where(x => x.ChapterId == chapterId);
        return this;
    }

    public IChapterPaymentAdjustmentQueryBuilder InCurrency(Guid currencyId)
    {
        Query = Query.Where(x => x.CurrencyId == currencyId);
        return this;
    }

    public IChapterPaymentAdjustmentQueryBuilder Outstanding()
    {
        Query = Query.Where(x => x.RecoveredAmount != x.Amount);
        return this;
    }
}
