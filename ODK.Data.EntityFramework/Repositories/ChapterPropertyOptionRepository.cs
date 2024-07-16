using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;
public class ChapterPropertyOptionRepository : ReadWriteRepositoryBase<ChapterPropertyOption>, IChapterPropertyOptionRepository
{
    public ChapterPropertyOptionRepository(OdkContext context) 
        : base(context)
    {
    }

    public IDeferredQueryMultiple<ChapterPropertyOption> GetByChapterId(Guid chapterId)
    {
        var query = 
            from chapterProperty in Set<ChapterProperty>()
            from chapterPropertyOption in Set()
            where chapterProperty.ChapterId == chapterId
                && chapterPropertyOption.ChapterPropertyId == chapterProperty.Id
            select chapterPropertyOption;
        return query.DeferredMultiple();
    }
}
