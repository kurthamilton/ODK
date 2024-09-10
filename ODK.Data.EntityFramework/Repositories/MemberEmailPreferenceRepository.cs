using ODK.Core.Members;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class MemberEmailPreferenceRepository : WriteRepositoryBase<MemberEmailPreference>, IMemberEmailPreferenceRepository
{
    public MemberEmailPreferenceRepository(OdkContext context) 
        : base(context)
    {
    }

    public IDeferredQueryMultiple<MemberEmailPreference> GetByMemberId(Guid memberId) => Set()
        .Where(x => x.MemberId == memberId)
        .DeferredMultiple();
}
