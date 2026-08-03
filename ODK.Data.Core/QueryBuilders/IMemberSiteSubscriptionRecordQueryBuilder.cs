using ODK.Core.Members;

namespace ODK.Data.Core.QueryBuilders;

public interface IMemberSiteSubscriptionRecordQueryBuilder :
    IDatabaseEntityQueryBuilder<MemberSiteSubscriptionRecord, IMemberSiteSubscriptionRecordQueryBuilder>
{
    IMemberSiteSubscriptionRecordQueryBuilder Current();

    IMemberSiteSubscriptionRecordQueryBuilder ForInitiator(string initiatorId);

    IMemberSiteSubscriptionRecordQueryBuilder ForMember(Guid memberId);

    IMemberSiteSubscriptionRecordQueryBuilder ForPayment(Guid paymentId);
}
