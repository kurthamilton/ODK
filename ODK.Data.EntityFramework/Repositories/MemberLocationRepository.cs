using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ODK.Core.Members;
using ODK.Data.Core.Repositories;

namespace ODK.Data.EntityFramework.Repositories;

public class MemberLocationRepository : WriteRepositoryBase<MemberLocation>, IMemberLocationRepository
{
    public MemberLocationRepository(OdkContext context) 
        : base(context)
    {
    }

    public Task<MemberLocation?> GetByMemberId(Guid memberId) => Set()
        .Where(x => x.MemberId == memberId)
        .FirstOrDefaultAsync();
}
