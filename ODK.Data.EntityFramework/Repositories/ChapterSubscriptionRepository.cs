using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Data.Core.Chapters;
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

    public IDeferredQueryMultiple<ChapterSubscriptionAdminDto> GetAdminDtosByChapterId(
        Guid chapterId, bool includeDisabled)
    {
        var query =
            from chapterSubscription in Set(chapterId, includeDisabled)
            from sitePaymentSettings in Set<SitePaymentSettings>()
                .Where(x => x.Id == chapterSubscription.SitePaymentSettingId)
                .DefaultIfEmpty()
            select new ChapterSubscriptionAdminDto
            {
                ChapterSubscription = chapterSubscription,
                SitePaymentSettings = sitePaymentSettings,
                Used = Set<MemberSubscriptionRecord>()
                    .Where(x => x.ChapterSubscriptionId == chapterSubscription.Id)
                    .Any()
            };

        return query.DeferredMultiple();
    }

    public IDeferredQueryMultiple<ChapterSubscription> GetByChapterId(Guid chapterId, bool includeDisabled)
        => Set(chapterId, includeDisabled).DeferredMultiple();

    public IDeferredQuerySingle<ChapterSubscriptionDto> GetDtoById(Guid id)
    {
        var query =
            from chapterSubscription in Set()
            from sitePaymentSettings in Set<SitePaymentSettings>()
                .Where(x => x.Id == chapterSubscription.SitePaymentSettingId)
                .DefaultIfEmpty()
            where chapterSubscription.Id == id
            select new ChapterSubscriptionDto
            {
                ChapterSubscription = chapterSubscription,
                SitePaymentSettings = sitePaymentSettings,
            };

        return query.DeferredSingle();
    }

    public IDeferredQueryMultiple<ChapterSubscriptionDto> GetDtosByChapterId(Guid chapterId, bool includeDisabled)
    {
        var query =
            from chapterSubscription in Set(chapterId, includeDisabled)
            from sitePaymentSettings in Set<SitePaymentSettings>()
                .Where(x => x.Id == chapterSubscription.SitePaymentSettingId)
                .DefaultIfEmpty()
            select new ChapterSubscriptionDto
            {
                ChapterSubscription = chapterSubscription,
                SitePaymentSettings = sitePaymentSettings
            };
        return query.DeferredMultiple();
    }

    public IDeferredQuery<bool> InUse(Guid chapterSubscriptionId)
        => Set<MemberSubscriptionRecord>()
            .Where(x => x.ChapterSubscriptionId == chapterSubscriptionId)
            .DeferredAny();

    private IQueryable<ChapterSubscription> Set(Guid chapterId, bool includeDisabled)
    {
        var query = Set()
            .Where(x => x.ChapterId == chapterId);

        if (!includeDisabled)
        {
            query = query.Where(x => !x.Disabled);
        }

        return query;
    }
}