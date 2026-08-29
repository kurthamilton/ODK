using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Data.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterSubscriptionRepository : ReadWriteRepositoryBase<ChapterSubscription>, IChapterSubscriptionRepository
{
    public ChapterSubscriptionRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<ChapterSubscriptionAdminDto> GetAdminDtosByChapterId(
        Guid chapterId, EnvironmentType environment, bool includeDisabled)
    {
        var query =
            from chapterSubscription in Set(chapterId, environment, includeDisabled)
            select new ChapterSubscriptionAdminDto
            {
                ChapterSubscription = chapterSubscription,
                Used = Set<MemberSubscriptionRecord>()
                    .Where(x => x.ChapterSubscriptionId == chapterSubscription.Id)
                    .Any()
            };

        return query.DeferredMultiple();
    }

    public IDeferredQueryMultiple<ChapterSubscription> GetByChapterId(
        Guid chapterId, EnvironmentType environment, bool includeDisabled)
        => Set(chapterId, environment, includeDisabled).DeferredMultiple();

    public IDeferredQuery<bool> InUse(Guid chapterSubscriptionId)
        => Set<MemberSubscriptionRecord>()
            .Where(x => x.ChapterSubscriptionId == chapterSubscriptionId)
            .DeferredAny();

    protected override IQueryable<ChapterSubscription> Set()
        => base.Set()
            .Include(x => x.Currency);

    private IQueryable<ChapterSubscription> Set(Guid chapterId, EnvironmentType environment, bool includeDisabled)
    {
        var query = Set()
            .Where(x => x.ChapterId == chapterId && x.Environment == environment);

        if (!includeDisabled)
        {
            query = query.Where(x => !x.Disabled);
        }

        return query;
    }
}