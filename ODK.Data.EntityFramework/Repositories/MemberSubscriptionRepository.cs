using ODK.Core.Members;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

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

    public IDeferredQueryMultiple<MemberSubscription> GetByChapterId(Guid chapterId) => Set()
        .Where(x => x.ChapterId ==  chapterId)
        .DeferredMultiple();

    public IDeferredQuerySingleOrDefault<MemberSubscription> GetByMemberId(Guid memberId, Guid chapterId) => Set()
        .Where(x => x.MemberId == memberId && x.ChapterId == chapterId)
        .DeferredSingleOrDefault();
}
