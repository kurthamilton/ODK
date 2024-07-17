using ODK.Core.Emails;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;
public class ChapterEmailRepository : ReadWriteRepositoryBase<ChapterEmail>, IChapterEmailRepository
{
    public ChapterEmailRepository(OdkContext context) 
        : base(context)
    {
    }

    public IDeferredQueryMultiple<ChapterEmail> GetByChapterId(Guid chapterId) => Set()
        .Where(x => x.ChapterId == chapterId)
        .DeferredMultiple();

    public IDeferredQuerySingleOrDefault<ChapterEmail> GetByChapterId(Guid chapterId, EmailType type) => Set()
        .Where(x => x.ChapterId == chapterId && x.Type == type)
        .DeferredSingleOrDefault();
}
