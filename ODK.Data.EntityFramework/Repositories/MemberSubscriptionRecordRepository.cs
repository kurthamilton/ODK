using Microsoft.EntityFrameworkCore;
using ODK.Core.Members;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.QueryBuilders;

namespace ODK.Data.EntityFramework.Repositories;

public class MemberSubscriptionRecordRepository :
    ReadWriteRepositoryBase<MemberSubscriptionRecord, IMemberSubscriptionRecordQueryBuilder>,
    IMemberSubscriptionRecordRepository
{
    public MemberSubscriptionRecordRepository(DbContext context)
        : base(context)
    {
    }

    public override IMemberSubscriptionRecordQueryBuilder Query()
        => CreateQueryBuilder(context => new MemberSubscriptionRecordQueryBuilder(context));
}
