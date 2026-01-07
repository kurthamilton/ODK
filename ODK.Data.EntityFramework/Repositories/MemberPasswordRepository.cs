using ODK.Core.Members;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class MemberPasswordRepository : WriteRepositoryBase<MemberPassword>, IMemberPasswordRepository
{
    public MemberPasswordRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQuerySingleOrDefault<MemberPassword> GetByMemberId(Guid memberId) => Set()
        .Where(x => x.MemberId == memberId)
        .DeferredSingleOrDefault();
}
