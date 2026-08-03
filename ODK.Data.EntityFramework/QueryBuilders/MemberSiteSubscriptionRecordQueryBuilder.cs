using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Subscriptions;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class MemberSiteSubscriptionRecordQueryBuilder :
    DatabaseEntityQueryBuilder<MemberSiteSubscriptionRecord, IMemberSiteSubscriptionRecordQueryBuilder>,
    IMemberSiteSubscriptionRecordQueryBuilder
{
    internal MemberSiteSubscriptionRecordQueryBuilder(DbContext context)
        : base(context)
    {
    }

    protected override MemberSiteSubscriptionRecordQueryBuilder Builder => this;

    public IMemberSiteSubscriptionRecordQueryBuilder Active()
    {
        Query = Query.Where(x => x.ExpiresUtc == null || x.ExpiresUtc > DateTime.UtcNow);
        return this;
    }

    public IMemberSiteSubscriptionRecordQueryBuilder Current()
    {
        Query = Query.Where(x => x.IsCurrent);
        return this;
    }

    public IMemberSiteSubscriptionRecordQueryBuilder ForChapterOwner(Guid chapterId)
    {
        Query =
            from record in Query
            from chapter in Set<Chapter>()
                .Where(x => x.OwnerId == record.MemberId)
            where chapter.Id == chapterId
            select record;

        return this;
    }

    public IMemberSiteSubscriptionRecordQueryBuilder ForExternalId(string externalId)
    {
        Query = Query.Where(x => x.ExternalId == externalId);
        return this;
    }

    public IMemberSiteSubscriptionRecordQueryBuilder ForInitiator(string initiatorId)
    {
        Query = Query.Where(x => x.InitiatorId == initiatorId);
        return this;
    }

    public IMemberSiteSubscriptionRecordQueryBuilder ForMember(Guid memberId)
    {
        Query = Query.Where(x => x.MemberId == memberId);
        return this;
    }

    public IMemberSiteSubscriptionRecordQueryBuilder ForPayment(Guid paymentId)
    {
        Query = Query.Where(x => x.PaymentId == paymentId);
        return this;
    }

    public IDeferredQuery<bool> HasFeature(SiteFeatureType feature)
        => SiteSubscription().HasFeature(feature);

    public ISiteSubscriptionQueryBuilder SiteSubscription()
    {
        var query =
            from record in Query
            from siteSubscription in Set<SiteSubscription>()
                .Where(x => x.Id == record.SiteSubscriptionId)
            select siteSubscription;

        return CreateQueryBuilder<ISiteSubscriptionQueryBuilder, SiteSubscription>(
            context => new SiteSubscriptionQueryBuilder(context, query));
    }

    public IQueryBuilder<MemberSiteSubscriptionState> ToState()
    {
        var query = Query.Select(x => new MemberSiteSubscriptionState
        {
            CancelledUtc = x.CancelledUtc,
            ExpiresUtc = x.ExpiresUtc,
            ExternalId = x.ExternalId,
            MemberId = x.MemberId!.Value,
            SiteSubscriptionId = x.SiteSubscriptionId,
            SiteSubscriptionPriceId = x.SiteSubscriptionPriceId
        });

        return ProjectTo(query);
    }
}
