using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class ChapterPaymentAccountQueryBuilder
    : DatabaseEntityQueryBuilder<ChapterPaymentAccount, IChapterPaymentAccountQueryBuilder>,
    IChapterPaymentAccountQueryBuilder
{
    public ChapterPaymentAccountQueryBuilder(DbContext context)
        : base(context)
    {
    }

    protected override IChapterPaymentAccountQueryBuilder Builder => this;

    public IChapterPaymentAccountQueryBuilder ForChapter(Guid chapterId)
    {
        Query = Query.Where(x => x.ChapterId == chapterId);
        return this;
    }

    public IChapterPaymentAccountQueryBuilder ForChapters(IEnumerable<Guid> chapterIds)
    {
        Query = Query.Where(x => chapterIds.Contains(x.ChapterId));
        return this;
    }

    public IChapterPaymentAccountQueryBuilder ForEnvironment(EnvironmentType environment)
    {
        Query = Query.Where(x => x.Environment == environment);
        return this;
    }
}
