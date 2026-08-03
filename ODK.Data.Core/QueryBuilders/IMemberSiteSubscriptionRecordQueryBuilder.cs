using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.QueryBuilders;

public interface IMemberSiteSubscriptionRecordQueryBuilder :
    IDatabaseEntityQueryBuilder<MemberSiteSubscriptionRecord, IMemberSiteSubscriptionRecordQueryBuilder>
{
    IMemberSiteSubscriptionRecordQueryBuilder Active();

    IMemberSiteSubscriptionRecordQueryBuilder Current();

    IMemberSiteSubscriptionRecordQueryBuilder ForChapterOwner(Guid chapterId);

    IMemberSiteSubscriptionRecordQueryBuilder ForExternalId(string externalId);

    IMemberSiteSubscriptionRecordQueryBuilder ForInitiator(string initiatorId);

    IMemberSiteSubscriptionRecordQueryBuilder ForMember(Guid memberId);

    IMemberSiteSubscriptionRecordQueryBuilder ForPayment(Guid paymentId);

    IDeferredQuery<bool> HasFeature(SiteFeatureType feature);

    ISiteSubscriptionQueryBuilder SiteSubscription();

    IQueryBuilder<MemberSiteSubscriptionState> ToState();
}
