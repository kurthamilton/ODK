using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterEmailProviderRepository : ReadWriteRepositoryBase<ChapterEmailProvider>, IChapterEmailProviderRepository
{
    public ChapterEmailProviderRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<ChapterEmailProvider> GetByChapterId(Guid chapterId) => Set()
        .Where(x => x.ChapterId == chapterId)
        .OrderBy(x => x.Order)
        .DeferredMultiple();
}
