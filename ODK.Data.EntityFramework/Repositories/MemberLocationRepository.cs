using Microsoft.EntityFrameworkCore;
using ODK.Core;
using ODK.Core.Members;
using ODK.Data.Core.Repositories;

namespace ODK.Data.EntityFramework.Repositories;

public class MemberLocationRepository : WriteRepositoryBase<MemberLocation>, IMemberLocationRepository
{
    public MemberLocationRepository(OdkContext context)
        : base(context)
    {
    }

    public async Task<MemberLocation> GetByMemberId(Guid memberId)
    {
        var memberLocation = await Set()
            .Where(x => x.MemberId == memberId)
            .FirstOrDefaultAsync();
        OdkAssertions.Exists(memberLocation);
        return memberLocation;
    }
}
