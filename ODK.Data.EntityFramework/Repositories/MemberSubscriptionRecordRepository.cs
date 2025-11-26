using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class MemberSubscriptionRecordRepository : ReadWriteRepositoryBase<MemberSubscriptionRecord>, IMemberSubscriptionRecordRepository
{
    public MemberSubscriptionRecordRepository(OdkContext context) 
        : base(context)
    {
    }

    public IDeferredQuerySingle<MemberSubscriptionRecord> GetByExternalId(string externalId)
        => Set()
            .Where(x => x.ExternalId == externalId)
            .DeferredSingle();

    public IDeferredQuerySingleOrDefault<MemberSubscriptionRecord> GetByExternalIdOrDefault(string externalId)
        => Set()
            .Where(x => x.ExternalId == externalId)
            .DeferredSingleOrDefault();

    public IDeferredQueryMultiple<MemberSubscriptionRecord> GetByExternalIds(IEnumerable<string> externalIds)
        => Set()
            .Where(x => externalIds.Contains(x.ExternalId))
            .DeferredMultiple();

    public IDeferredQuerySingleOrDefault<MemberSubscriptionRecord> GetLatest(Guid memberId, Guid chapterId)
    {
        var query =
            from memberSubscriptionRecord in Set()            
            where memberSubscriptionRecord.MemberId == memberId && memberSubscriptionRecord.ChapterId == chapterId
            select memberSubscriptionRecord;

        return query
            .OrderByDescending(x => x.PurchasedUtc)
            .DeferredSingleOrDefault();
    }
}
