using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Subscriptions;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Members;

namespace ODK.Data.Core.QueryBuilders;

public interface IMemberSiteSubscriptionRecordQueryBuilder :
    IDatabaseEntityQueryBuilder<MemberSiteSubscriptionRecord, IMemberSiteSubscriptionRecordQueryBuilder>
{
    /// <summary>
    /// Records that have not expired, treating one that expired within <paramref name="cooldown"/> as active.
    /// </summary>
    IMemberSiteSubscriptionRecordQueryBuilder Active(SiteSubscriptionCooldown cooldown);

    IMemberSiteSubscriptionRecordQueryBuilder Current();

    IMemberSiteSubscriptionRecordQueryBuilder ForChapterOwner(Guid chapterId);

    IMemberSiteSubscriptionRecordQueryBuilder ForExternalId(string externalId);

    IMemberSiteSubscriptionRecordQueryBuilder ForInitiator(string initiatorId);

    IMemberSiteSubscriptionRecordQueryBuilder ForMember(Guid memberId);

    IMemberSiteSubscriptionRecordQueryBuilder ForPayment(Guid paymentId);

    IMemberSiteSubscriptionRecordQueryBuilder ForSiteSubscription(Guid siteSubscriptionId);

    IMemberSiteSubscriptionRecordQueryBuilder ForSiteSubscriptionPrice(Guid siteSubscriptionPriceId);

    IDeferredQuery<bool> HasFeature(SiteFeatureType feature);

    ISiteSubscriptionQueryBuilder SiteSubscription();

    /// <summary>
    /// The prices these records name, one row per price however many records name it. Records without a
    /// price - a free subscription takes none - contribute nothing.
    /// </summary>
    IQueryBuilder<SiteSubscriptionPrice> SiteSubscriptionPrices();

    IQueryBuilder<MemberSiteSubscriptionDto> ToDto();

    IQueryBuilder<MemberSiteSubscriptionState> ToState();
}
