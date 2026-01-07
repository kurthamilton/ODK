using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class MemberSiteSubscriptionRepository : ReadWriteRepositoryBase<MemberSiteSubscription>, IMemberSiteSubscriptionRepository
{
    public MemberSiteSubscriptionRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<MemberSiteSubscription> GetAllChapterOwnerSubscriptions(PlatformType platform)
    {
        var query =
            from chapter in Set<Chapter>()
            from subscription in Set()
                .Where(x => x.MemberId == chapter.OwnerId && x.SiteSubscription.Platform == platform)
            select subscription;
        return query.DeferredMultiple();
    }

    public IDeferredQuerySingleOrDefault<MemberSiteSubscription> GetByChapterId(Guid chapterId)
    {
        var query =
            from chapter in Set<Chapter>()
            from subscription in Set()
            where chapter.Id == chapterId
                && subscription.MemberId == chapter.OwnerId
            select subscription;

        return query.DeferredSingleOrDefault();
    }

    public IDeferredQuerySingleOrDefault<MemberSiteSubscription> GetByMemberId(Guid memberId, PlatformType platform) => Set()
        .Where(x => x.MemberId == memberId && x.SiteSubscription.Platform == platform)
        .DeferredSingleOrDefault();

    public IDeferredQueryMultiple<MemberSiteSubscription> GetByMemberId(Guid memberId) => Set()
        .Where(x => x.MemberId == memberId)
        .DeferredMultiple();

    public IDeferredQueryMultiple<MemberSiteSubscription> GetExpired() => Set()
        .Where(x => x.ExpiresUtc <= DateTime.UtcNow)
        .DeferredMultiple();

    protected override IQueryable<MemberSiteSubscription> Set() => base.Set()
        .Include(x => x.SiteSubscription)
        .Include(x => x.SiteSubscriptionPrice);
}
