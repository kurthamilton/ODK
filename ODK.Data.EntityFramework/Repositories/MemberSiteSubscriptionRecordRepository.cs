using Microsoft.EntityFrameworkCore;
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

public class MemberSiteSubscriptionRecordRepository :
    ReadWriteRepositoryBase<MemberSiteSubscriptionRecord, IMemberSiteSubscriptionRecordQueryBuilder>,
    IMemberSiteSubscriptionRecordRepository
{
    public MemberSiteSubscriptionRecordRepository(DbContext context)
        : base(context)
    {
    }

    /* The platform scopes the chapters only, never the plan their owner is on. A record is per member and
       names whichever plan they hold, so a platform term on the subscription would drop the owner - this
       being an inner join - rather than narrow what is read about them: on Group Squirrel, where the
       chapter scope takes in Drunken Knitwits groups, every one of them would vanish from a list that had
       already decided to show it. */
    public IDeferredQueryMultiple<MemberSiteSubscriptionDto> GetAllChapterOwnerSubscriptionDtos(PlatformType platform)
    {
        var query =
            from chapter in Set<Chapter>()
                .VisibleOn(platform, includeUnpublished: true)
            from record in Set()
                .Where(x => x.IsCurrent && x.MemberId == chapter.OwnerId)
            from siteSubscription in Set<SiteSubscription>()
                .Where(x => x.Id == record.SiteSubscriptionId)
            from siteSubscriptionPrice in Set<SiteSubscriptionPrice>()
                .Where(x => x.Id == record.SiteSubscriptionPriceId)
                .DefaultIfEmpty()
            select new MemberSiteSubscriptionDto
            {
                MemberSiteSubscription = new MemberSiteSubscriptionState
                {
                    CancelledUtc = record.CancelledUtc,
                    ExpiresUtc = record.ExpiresUtc,
                    ExternalId = record.ExternalId,
                    MemberId = record.MemberId,
                    SiteSubscriptionId = record.SiteSubscriptionId,
                    SiteSubscriptionPriceId = record.SiteSubscriptionPriceId
                },
                SiteSubscription = siteSubscription,
                SiteSubscriptionPrice = siteSubscriptionPrice
            };
        return query.DeferredMultiple();
    }

    public IDeferredQuerySingleOrDefault<MemberSiteSubscriptionDto> GetDtoByMemberId(Guid memberId)
        => Query()
            .Current()
            .ForMember(memberId)
            .ToDto()
            .GetSingleOrDefault();

    public override IMemberSiteSubscriptionRecordQueryBuilder Query()
        => CreateQueryBuilder(context => new MemberSiteSubscriptionRecordQueryBuilder(context));
}
