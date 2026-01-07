using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterPropertyRepository : ReadWriteRepositoryBase<ChapterProperty>, IChapterPropertyRepository
{
    public ChapterPropertyRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQuery<bool> ChapterHasProperties(Guid chapterId)
        => Set()
            .Where(x => x.ChapterId == chapterId)
            .DeferredAny();

    public IDeferredQueryMultiple<ChapterProperty> GetByChapterId(Guid chapterId)
        => Set()
            .Where(x => x.ChapterId == chapterId)
            .DeferredMultiple();
}
