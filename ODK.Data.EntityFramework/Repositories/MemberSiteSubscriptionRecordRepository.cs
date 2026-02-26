using Microsoft.EntityFrameworkCore;
using ODK.Core.Members;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Repositories;
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

    public override IMemberSiteSubscriptionRecordQueryBuilder Query()
        => CreateQueryBuilder<IMemberSiteSubscriptionRecordQueryBuilder, MemberSiteSubscriptionRecord>(
            context => new MemberSiteSubscriptionRecordQueryBuilder(context));
}