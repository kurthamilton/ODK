using ODK.Core.Chapters;
using ODK.Core.Pages;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterPageRepository : ReadWriteRepositoryBase<ChapterPage>, IChapterPageRepository
{
    public ChapterPageRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<ChapterPage> GetByChapterId(Guid chapterId)
        => Set()
            .Where(x => x.ChapterId == chapterId)
            .DeferredMultiple();

    public IDeferredQuerySingleOrDefault<ChapterPage> GetByChapterId(Guid chapterId, PageType pageType)
        => Set()
            .Where(x => x.ChapterId == chapterId && x.PageType == pageType)
            .DeferredSingleOrDefault();
}