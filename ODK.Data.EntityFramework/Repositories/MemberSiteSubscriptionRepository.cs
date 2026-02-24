using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Members;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;
using ODK.Data.EntityFramework.Queries;
using ODK.Data.EntityFramework.QueryBuilders;

namespace ODK.Data.EntityFramework.Repositories;

public class MemberSiteSubscriptionRepository
    : ReadWriteRepositoryBase<MemberSiteSubscription, IMemberSiteSubscriptionQueryBuilder>, IMemberSiteSubscriptionRepository
{
    public MemberSiteSubscriptionRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<MemberSiteSubscriptionDto> GetAllChapterOwnerSubscriptionDtos(PlatformType platform)
    {
        var query =
            from chapter in Set<Chapter>()
                .ForPlatform(platform, includeUnpublished: true)
            from memberSiteSubscription in Set()
                .Where(x => x.MemberId == chapter.OwnerId)
            from siteSubscription in Set<SiteSubscription>()
                .Where(x => x.Id == memberSiteSubscription.SiteSubscriptionId)
            from siteSubscriptionPrice in Set<SiteSubscriptionPrice>()
                .Where(x => x.Id == memberSiteSubscription.SiteSubscriptionPriceId)
                .DefaultIfEmpty()
            where siteSubscription.Platform == platform
            select new MemberSiteSubscriptionDto
            {
                MemberSiteSubscription = memberSiteSubscription,
                SiteSubscription = siteSubscription,
                SiteSubscriptionPrice = siteSubscriptionPrice
            };
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

    public IDeferredQuerySingleOrDefault<MemberSiteSubscription> GetByMemberId(Guid memberId, PlatformType platform)
        => Query()
            .ForMember(memberId, platform)
            .GetSingleOrDefault();

    public IDeferredQueryMultiple<MemberSiteSubscription> GetByMemberId(Guid memberId)
        => Set()
            .Where(x => x.MemberId == memberId)
            .DeferredMultiple();

    public IDeferredQuerySingleOrDefault<MemberSiteSubscriptionDto> GetDtoByChapterId(Guid chapterId)
        => Query()
            .ForChapterOwner(chapterId)
            .ToMemberSiteSubscriptionDto()
            .GetSingleOrDefault();

    public IDeferredQuerySingleOrDefault<MemberSiteSubscriptionDto> GetDtoByMemberId(Guid memberId, PlatformType platform)
        => Query()
            .ForMember(memberId, platform)
            .ToMemberSiteSubscriptionDto()
            .GetSingleOrDefault();

    public IDeferredQueryMultiple<MemberSiteSubscription> GetExpired()
        => Set()
            .Where(x => x.ExpiresUtc <= DateTime.UtcNow)
            .DeferredMultiple();

    public override IMemberSiteSubscriptionQueryBuilder Query()
        => CreateQueryBuilder<IMemberSiteSubscriptionQueryBuilder, MemberSiteSubscription>(
            context => new MemberSiteSubscriptionQueryBuilder(context));
}