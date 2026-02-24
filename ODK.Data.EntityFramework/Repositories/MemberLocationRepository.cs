using Microsoft.EntityFrameworkCore;
using ODK.Core.Members;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class MemberLocationRepository : WriteRepositoryBase<MemberLocation>, IMemberLocationRepository
{
    public MemberLocationRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQuerySingle<MemberLocation> GetByMemberId(Guid memberId)
        => Set()
            .Where(x => x.MemberId == memberId)
            .DeferredSingle();
}
