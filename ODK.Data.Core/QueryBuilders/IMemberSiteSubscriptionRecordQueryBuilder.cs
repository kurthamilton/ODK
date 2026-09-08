using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Platforms;
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

    /// <summary>
    /// Records that have expired, treating one that expired within <paramref name="cooldown"/> as active.
    /// The exact complement of <see cref="Active"/>: a record with no expiry never expires, so it belongs
    /// to neither. The two have to stay in step.
    /// </summary>
    IMemberSiteSubscriptionRecordQueryBuilder Expired(SiteSubscriptionCooldown cooldown);

    IMemberSiteSubscriptionRecordQueryBuilder ForChapterOwner(Guid chapterId);

    /// <summary>Records on a plan belonging to the environment - the plan's, not the member's.</summary>
    IMemberSiteSubscriptionRecordQueryBuilder ForEnvironment(EnvironmentType environment);

    IMemberSiteSubscriptionRecordQueryBuilder ForExternalId(string externalId);

    IMemberSiteSubscriptionRecordQueryBuilder ForInitiator(string initiatorId);

    IMemberSiteSubscriptionRecordQueryBuilder ForMember(Guid memberId);

    IMemberSiteSubscriptionRecordQueryBuilder ForPayment(Guid paymentId);

    /// <summary>
    /// Records on a plan belonging to the platform. The plan's platform, not the member's: a plan belongs
    /// to the platform that sells it, whichever site the member signed up on.
    /// </summary>
    IMemberSiteSubscriptionRecordQueryBuilder ForPlatform(PlatformType platform);

    IMemberSiteSubscriptionRecordQueryBuilder ForSiteSubscription(Guid siteSubscriptionId);

    IMemberSiteSubscriptionRecordQueryBuilder ForSiteSubscriptionPrice(Guid siteSubscriptionPriceId);

    /// <inheritdoc cref="IMemberSubscriptionRecordQueryBuilder.HasExternalId"/>
    IMemberSiteSubscriptionRecordQueryBuilder HasExternalId();

    IDeferredQuery<bool> HasFeature(SiteFeatureType feature);

    /// <summary>
    /// The most recently created of the matching records, and only that one. Not <see cref="Current"/>,
    /// which is the flag saying which record is in force: this orders whatever a filter has left, so a
    /// caller can take the newest record of some other kind.
    /// </summary>
    IMemberSiteSubscriptionRecordQueryBuilder MostRecent();

    ISiteSubscriptionQueryBuilder SiteSubscription();

    /// <summary>
    /// The prices these records name, one row per price however many records name it. Records without a
    /// price - a free subscription takes none - contribute nothing.
    /// </summary>
    IQueryBuilder<SiteSubscriptionPrice> SiteSubscriptionPrices();

    IQueryBuilder<MemberSiteSubscriptionDto> ToDto();

    IQueryBuilder<MemberSiteSubscriptionState> ToState();
}
