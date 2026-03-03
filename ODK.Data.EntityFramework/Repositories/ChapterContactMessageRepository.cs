using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;
using ODK.Data.EntityFramework.QueryBuilders;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterContactMessageRepository
    : ReadWriteRepositoryBase<ChapterContactMessage, IChapterContactMessageQueryBuilder>, IChapterContactMessageRepository
{
    public ChapterContactMessageRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<ChapterContactMessage> GetByChapterId(Guid chapterId) => Set()
        .Where(x => x.ChapterId == chapterId)
        .DeferredMultiple();

    public override IChapterContactMessageQueryBuilder Query()
        => CreateQueryBuilder(context => new ChapterContactMessageQueryBuilder(context));
}