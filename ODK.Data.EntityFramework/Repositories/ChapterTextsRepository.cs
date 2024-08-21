using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;
public class ChapterTextsRepository : WriteRepositoryBase<ChapterTexts>, IChapterTextsRepository
{
    public ChapterTextsRepository(OdkContext context) 
        : base(context)
    {
    }

    public IDeferredQuerySingleOrDefault<ChapterTexts> GetByChapterId(Guid chapterId) => Set()
        .Where(x => x.ChapterId == chapterId)
        .DeferredSingleOrDefault();

    public IDeferredQueryMultiple<ChapterTexts> GetByChapterIds(IEnumerable<Guid> chapterIds) => Set()
        .Where(x => chapterIds.Contains(x.ChapterId))
        .DeferredMultiple();
}
