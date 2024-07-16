using ODK.Core.Members;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;
public class MemberPropertyRepository : ReadWriteRepositoryBase<MemberProperty>, IMemberPropertyRepository
{
    public MemberPropertyRepository(OdkContext context) 
        : base(context)
    {
    }

    public IDeferredQueryMultiple<MemberProperty> GetByMemberId(Guid memberId) => Set()
        .Where(x => x.MemberId == memberId)
        .DeferredMultiple();
}
