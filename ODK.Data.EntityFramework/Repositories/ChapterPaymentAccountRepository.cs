using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterPaymentAccountRepository : ReadWriteRepositoryBase<ChapterPaymentAccount>, IChapterPaymentAccountRepository
{
    public ChapterPaymentAccountRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQuerySingleOrDefault<ChapterPaymentAccount> GetByChapterId(Guid chapterId)
        => Set()
            .Where(x => x.ChapterId == chapterId)
            .DeferredSingleOrDefault();
}