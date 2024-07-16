using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;
public class ChapterSubscriptionRepository : ReadWriteRepositoryBase<ChapterSubscription>, IChapterSubscriptionRepository
{
    public ChapterSubscriptionRepository(OdkContext context) 
        : base(context)
    {
    }

    public IDeferredQueryMultiple<ChapterSubscription> GetByChapterId(Guid chapterId) => Set()
        .Where(x => x.ChapterId == chapterId)
        .DeferredMultiple();
}
