using Microsoft.EntityFrameworkCore;
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

    public IDeferredQueryMultiple<MemberSubscription> GetByChapterId(Guid chapterId) => Set()
        .Where(x => x.MemberChapter.ChapterId ==  chapterId)
        .DeferredMultiple();

    public IDeferredQuerySingleOrDefault<MemberSubscription> GetByMemberId(Guid memberId, Guid chapterId) => Set()
        .Where(x => x.MemberChapter.MemberId == memberId && x.MemberChapter.ChapterId == chapterId)
        .DeferredSingleOrDefault();

    protected override IQueryable<MemberSubscription> Set() => base.Set()
        .Include(x => x.MemberChapter);
}
