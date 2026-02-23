using Microsoft.EntityFrameworkCore;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.Core.Subscriptions;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class SiteSubscriptionRepository : ReadWriteRepositoryBase<SiteSubscription>, ISiteSubscriptionRepository
{
    public SiteSubscriptionRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<SiteSubscription> GetAll(PlatformType platform) => Set()
        .Where(x => x.Platform == platform)
        .DeferredMultiple();

    public IDeferredQueryMultiple<SiteSubscription> GetAllEnabled(PlatformType platform) => Set()
        .Where(x => x.Platform == platform && x.Enabled)
        .DeferredMultiple();

    public IDeferredQuerySingle<SiteSubscription> GetByPriceId(Guid priceId)
    {
        var query =
            from price in Set<SiteSubscriptionPrice>()
            from siteSubscription in Set()
                .Where(x => x.Id == price.SiteSubscriptionId)
            where price.Id == priceId
            select siteSubscription;

        return query.DeferredSingle();
    }

    public IDeferredQuerySingle<SiteSubscription> GetDefault(PlatformType platform)
        => Set()
            .Where(x => x.Platform == platform && x.Enabled && x.Default)
            .DeferredSingle();

    public IDeferredQueryMultiple<SiteSubscriptionSummaryDto> GetSummaries(PlatformType platform)
    {
        var query =
            from siteSubscription in Set()
            where siteSubscription.Platform == platform
            select new SiteSubscriptionSummaryDto
            {
                ActiveMemberSiteSubscriptionCount = Set<MemberSiteSubscription>()
                    .Where(x => x.SiteSubscriptionId == siteSubscription.Id && x.ExpiresUtc > DateTime.UtcNow)
                    .Count(),
                SiteSubscription = siteSubscription
            };

        return query.DeferredMultiple();
    }

    protected override IQueryable<SiteSubscription> Set()
        => base.Set()
            .Include(x => x.Features);
}