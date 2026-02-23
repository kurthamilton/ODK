using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Data.Core.Members;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class MemberSiteSubscriptionQueryBuilder
    : DatabaseEntityQueryBuilder<MemberSiteSubscription, IMemberSiteSubscriptionQueryBuilder>, IMemberSiteSubscriptionQueryBuilder
{
    public MemberSiteSubscriptionQueryBuilder(OdkContext context)
        : base(context)
    {
    }

    protected override IMemberSiteSubscriptionQueryBuilder Builder => this;

    public IMemberSiteSubscriptionQueryBuilder Active()
    {
        Query = Query.Where(x => x.ExpiresUtc > DateTime.UtcNow);
        return this;
    }

    public IMemberSiteSubscriptionQueryBuilder ForChapterOwner(Guid chapterId)
    {
        Query =
            from memberSiteSubscription in Query
            from chapter in Set<Chapter>()
                .Where(x => x.OwnerId == memberSiteSubscription.MemberId)
            where chapter.Id == chapterId
            select memberSiteSubscription;

        return this;
    }

    public IMemberSiteSubscriptionQueryBuilder ForMember(Guid memberId, PlatformType platform)
    {
        Query =
            from memberSiteSubscription in Query
            from siteSubscription in Set<SiteSubscription>()
                .Where(x => x.Id == memberSiteSubscription.SiteSubscriptionId)
            where memberSiteSubscription.MemberId == memberId && siteSubscription.Platform == platform
            select memberSiteSubscription;

        return this;
    }

    public IQueryBuilder<SiteSubscription> SiteSubscription()
    {
        var query =
            from memberSiteSubscription in Query
            from siteSubscription in Set<SiteSubscription>()
                .Where(x => x.Id == memberSiteSubscription.SiteSubscriptionId)
            select siteSubscription;

        return ProjectTo(query);
    }

    public IQueryBuilder<MemberSiteSubscriptionDto> ToMemberSiteSubscriptionDto()
    {
        var query =
            from memberSiteSubscription in Query
            from siteSubscription in Set<SiteSubscription>()
                .Where(x => x.Id == memberSiteSubscription.SiteSubscriptionId)
            from siteSubsriptionPrice in Set<SiteSubscriptionPrice>()
                .Where(x => x.Id == memberSiteSubscription.SiteSubscriptionPriceId)
                .DefaultIfEmpty()
            select new MemberSiteSubscriptionDto
            {
                MemberSiteSubscription = memberSiteSubscription,
                SiteSubscription = siteSubscription,
                SiteSubscriptionPrice = siteSubsriptionPrice
            };

        return ProjectTo(query);
    }
}