using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.QueryBuilders;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterPaymentAdjustmentRepository
    : ReadWriteRepositoryBase<ChapterPaymentAdjustment, IChapterPaymentAdjustmentQueryBuilder>,
    IChapterPaymentAdjustmentRepository
{
    public ChapterPaymentAdjustmentRepository(DbContext context)
        : base(context)
    {
    }

    public override IChapterPaymentAdjustmentQueryBuilder Query()
        => CreateQueryBuilder(context => new ChapterPaymentAdjustmentQueryBuilder(context));
}
