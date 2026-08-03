using Microsoft.EntityFrameworkCore;
using ODK.Core.Members;
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

    public IMemberSiteSubscriptionRecordQueryBuilder Current()
    {
        Query = Query.Where(x => x.IsCurrent);
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
}