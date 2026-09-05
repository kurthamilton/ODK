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
        => ByPriceId(priceId).DeferredSingle();

    public IDeferredQuerySingleOrDefault<SiteSubscription> GetByPriceIdOrDefault(Guid priceId)
        => ByPriceId(priceId).DeferredSingleOrDefault();

    public IDeferredQuerySingle<SiteSubscription> GetDefault(PlatformType platform)
        => Set()
            .Where(x => x.Platform == platform && x.Enabled && x.Default)
            .DeferredSingle();

    public IDeferredQueryMultiple<SiteSubscriptionSummaryDto> GetSummaries(
        PlatformType platform, SiteSubscriptionCooldown cooldown)
    {
        // Resolved here rather than in the predicate, which has to translate to SQL.
        var activeAfterUtc = cooldown.ActiveAfterUtc(DateTime.UtcNow);

        var query =
            from siteSubscription in Set()
            where siteSubscription.Platform == platform
            select new SiteSubscriptionSummaryDto
            {
                ActiveMemberSiteSubscriptionCount = Set<MemberSiteSubscriptionRecord>()
                    .Where(x => x.IsCurrent &&
                        x.SiteSubscriptionId == siteSubscription.Id &&
                        x.ExpiresUtc > activeAfterUtc)
                    .Count(),
                Features = Set<SiteSubscriptionFeature>()
                    .Where(x => x.SiteSubscriptionId == siteSubscription.Id)
                    .ToArray(),
                MemberSiteSubscriptionCount = Set<MemberSiteSubscriptionRecord>()
                    .Where(x => x.SiteSubscriptionId == siteSubscription.Id)
                    .Count(),
                SiteSubscription = siteSubscription
            };

        return query.DeferredMultiple();
    }

    public override ISiteSubscriptionQueryBuilder Query()
        => CreateQueryBuilder(context => new SiteSubscriptionQueryBuilder(context));

    private IQueryable<SiteSubscription> ByPriceId(Guid priceId)
        => from price in Set<SiteSubscriptionPrice>()
           from siteSubscription in Set()
               .Where(x => x.Id == price.SiteSubscriptionId)
           where price.Id == priceId
           select siteSubscription;
}