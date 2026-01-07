using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterContactMessageRepository : ReadWriteRepositoryBase<ChapterContactMessage>, IChapterContactMessageRepository
{
    public ChapterContactMessageRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<ChapterContactMessage> GetByChapterId(Guid chapterId) => Set()
        .Where(x => x.ChapterId == chapterId)
        .DeferredMultiple();
}
