using ODK.Core.Members;

namespace ODK.Data.Core.QueryBuilders;

public interface IMemberSiteSubscriptionRecordQueryBuilder :
    IDatabaseEntityQueryBuilder<MemberSiteSubscriptionRecord, IMemberSiteSubscriptionRecordQueryBuilder>
{
    IMemberSiteSubscriptionRecordQueryBuilder ForInitiator(string initiatorId);

    IMemberSiteSubscriptionRecordQueryBuilder ForPayment(Guid paymentId);
}
