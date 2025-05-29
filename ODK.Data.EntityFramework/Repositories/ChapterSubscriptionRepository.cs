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

    public IDeferredQueryMultiple<ChapterSubscription> GetByChapterId(Guid chapterId, bool includeDisabled)
    {
        var query = Set()
            .Where(x => x.ChapterId == chapterId);

        if (!includeDisabled)
        {
            query = query.Where(x => !x.Disabled);
        }

        return query.DeferredMultiple();
    }
}
