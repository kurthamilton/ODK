using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.Core.Repositories;

public interface IChapterPaymentAccountRepository
    : IReadWriteRepository<ChapterPaymentAccount, IChapterPaymentAccountQueryBuilder>
{
    IDeferredQuerySingleOrDefault<ChapterPaymentAccount> GetByChapterId(Guid chapterId);
}
