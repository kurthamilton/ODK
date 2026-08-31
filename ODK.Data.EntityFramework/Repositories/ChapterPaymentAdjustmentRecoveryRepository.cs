using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.QueryBuilders;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterPaymentAdjustmentRecoveryRepository
    : ReadWriteRepositoryBase<ChapterPaymentAdjustmentRecovery, IChapterPaymentAdjustmentRecoveryQueryBuilder>,
    IChapterPaymentAdjustmentRecoveryRepository
{
    public ChapterPaymentAdjustmentRecoveryRepository(DbContext context)
        : base(context)
    {
    }

    public override IChapterPaymentAdjustmentRecoveryQueryBuilder Query()
        => CreateQueryBuilder(context => new ChapterPaymentAdjustmentRecoveryQueryBuilder(context));
}
