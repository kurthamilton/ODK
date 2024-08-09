using Microsoft.EntityFrameworkCore;
using ODK.Core.Members;
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

    public IDeferredQuerySingleOrDefault<MemberSiteSubscription> GetByMemberId(Guid memberId) => Set()
        .Where(x => x.MemberId == memberId)
        .DeferredSingleOrDefault();

    protected override IQueryable<MemberSiteSubscription> Set() => base.Set()
        .Include(x => x.SiteSubscription);
}
