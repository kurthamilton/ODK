using Microsoft.EntityFrameworkCore;
using ODK.Core.Features;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Subscriptions;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class SiteSubscriptionQueryBuilder
    : DatabaseEntityQueryBuilder<SiteSubscription, ISiteSubscriptionQueryBuilder>, ISiteSubscriptionQueryBuilder
{
    public SiteSubscriptionQueryBuilder(DbContext context)
        : base(context)
    {
    }

    public SiteSubscriptionQueryBuilder(DbContext context, IQueryable<SiteSubscription> query)
        : base(context, query)
    {
    }

    protected override ISiteSubscriptionQueryBuilder Builder => this;

    public ISiteSubscriptionQueryBuilder Enabled()
    {
        Query = Query.Where(x => x.Enabled);
        return this;
    }

    public IQueryBuilder<SiteSubscriptionFeature> Features()
    {
        var query =
            from siteSubscription in Query
            from siteSubscriptionFeature in Set<SiteSubscriptionFeature>()
                .Where(x => x.SiteSubscriptionId == siteSubscription.Id)
            select siteSubscriptionFeature;

        return ProjectTo(query);
    }

    public IDeferredQuery<bool> HasFeature(SiteFeatureType feature)
    {
        var query =
            from siteSubscription in Query
            from siteSubscriptionFeature in Set<SiteSubscriptionFeature>()
                .Where(x => x.SiteSubscriptionId == siteSubscription.Id)
            where siteSubscriptionFeature.Feature == feature
            select siteSubscriptionFeature;

        return query.DeferredAny();
    }

    public ISiteSubscriptionQueryBuilder ForPlatform(PlatformType platform)
    {
        Query = Query.Where(x => x.Platform == platform);
        return this;
    }

    public IQueryBuilder<SiteSubscriptionWithFeaturesDto> WithFeatures()
    {
        var query =
            from siteSubscription in Query
            select new SiteSubscriptionWithFeaturesDto
            {
                Features =
                    Set<SiteSubscriptionFeature>()
                        .Where(x => x.SiteSubscriptionId == siteSubscription.Id)
                        .ToArray(),
                SiteSubscription = siteSubscription,
            };

        return ProjectTo(query);
    }
}