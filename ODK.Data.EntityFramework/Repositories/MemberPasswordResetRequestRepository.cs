using ODK.Core.Members;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;
public class MemberPasswordResetRequestRepository : WriteRepositoryBase<MemberPasswordResetRequest>, IMemberPasswordResetRequestRepository
{
    public MemberPasswordResetRequestRepository(OdkContext context) 
        : base(context)
    {
    }

    public IDeferredQuerySingleOrDefault<MemberPasswordResetRequest> GetByToken(string token) => Set()
        .Where(x => x.Token == token)
        .DeferredSingleOrDefault();
}
