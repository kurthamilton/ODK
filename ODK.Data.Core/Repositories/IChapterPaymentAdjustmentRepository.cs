using ODK.Core.Chapters;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.Core.Repositories;

public interface IChapterPaymentAdjustmentRepository
    : IReadWriteRepository<ChapterPaymentAdjustment, IChapterPaymentAdjustmentQueryBuilder>
{
}
