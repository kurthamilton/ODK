using Microsoft.EntityFrameworkCore;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class MemberSiteSubscriptionRepository : WriteRepositoryBase<MemberSiteSubscription>, IMemberSiteSubscriptionRepository
{
    public MemberSiteSubscriptionRepository(OdkContext context) 
        : base(context)
    {
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
