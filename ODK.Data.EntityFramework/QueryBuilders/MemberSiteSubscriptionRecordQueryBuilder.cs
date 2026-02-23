using ODK.Core.Members;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class MemberSiteSubscriptionRecordQueryBuilder : 
    DatabaseEntityQueryBuilder<MemberSiteSubscriptionRecord, IMemberSiteSubscriptionRecordQueryBuilder>, 
    IMemberSiteSubscriptionRecordQueryBuilder
{
    internal MemberSiteSubscriptionRecordQueryBuilder(OdkContext context) 
        : base(context)
    {
    }

    protected override MemberSiteSubscriptionRecordQueryBuilder Builder => this;

    public IMemberSiteSubscriptionRecordQueryBuilder ForPayment(Guid paymentId)
    {
        Query = Query.Where(x => x.PaymentId == paymentId);
        return this;
    }
}
