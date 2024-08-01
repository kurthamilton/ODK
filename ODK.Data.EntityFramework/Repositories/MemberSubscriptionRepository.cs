using ODK.Core.Members;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;
using ODK.Data.EntityFramework.Queries;

namespace ODK.Data.EntityFramework.Repositories;
public class MemberSubscriptionRepository : WriteRepositoryBase<MemberSubscription>, IMemberSubscriptionRepository
{
    public MemberSubscriptionRepository(OdkContext context) 
        : base(context)
    {
    }

    public void AddMemberSubscriptionRecord(MemberSubscriptionRecord record)
    {
        if (record.Id == default)
        {
            record.Id = Guid.NewGuid();
        }

        AddSingle(record);
    }

    public IDeferredQueryMultiple<MemberSubscription> GetByChapterId(Guid chapterId)
    {
        var query = 
            from memberSubscription in Set()
            from member in Set<Member>().InChapter(chapterId)
            where memberSubscription.MemberId == member.Id
            select memberSubscription;
        return query.DeferredMultiple();
    }

    public IDeferredQuerySingle<MemberSubscription> GetByMemberId(Guid memberId, Guid chapterId) => Set()
        .Where(x => x.MemberId == memberId)
        .OrderByDescending(x => x.ExpiresUtc)
        .DeferredSingle();
}
