using Microsoft.EntityFrameworkCore;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Repositories;
using ODK.Data.Core.Subscriptions;
using ODK.Data.EntityFramework.Extensions;
using ODK.Data.EntityFramework.QueryBuilders;

namespace ODK.Data.EntityFramework.Repositories;

public class SiteSubscriptionRepository
    : ReadWriteRepositoryBase<SiteSubscription, ISiteSubscriptionQueryBuilder>, ISiteSubscriptionRepository
{
    public SiteSubscriptionRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<SiteSubscription> GetAll(PlatformType platform)
        => Set()
            .Where(x => x.Platform == platform)
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
                Features = Set<SiteSubscriptionFeature>()
                    .Where(x => x.SiteSubscriptionId == siteSubscription.Id)
                    .ToArray(),
                SiteSubscription = siteSubscription
            };

        return query.DeferredMultiple();
    }

    public override ISiteSubscriptionQueryBuilder Query()
        => CreateQueryBuilder(context => new SiteSubscriptionQueryBuilder(context));
}