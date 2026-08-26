using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Data.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.QueryBuilders;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterPaymentAccountRepository :
    ReadWriteRepositoryBase<ChapterPaymentAccount, IChapterPaymentAccountQueryBuilder>, IChapterPaymentAccountRepository
{
    public ChapterPaymentAccountRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQuerySingleOrDefault<ChapterPaymentAccount> GetByChapterId(Guid chapterId)
        => Query()
            .ForChapter(chapterId)
            .GetSingleOrDefault();

    public override IChapterPaymentAccountQueryBuilder Query()
        => CreateQueryBuilder(context => new ChapterPaymentAccountQueryBuilder(context));
}