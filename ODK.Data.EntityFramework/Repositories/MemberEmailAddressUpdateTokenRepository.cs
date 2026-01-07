using ODK.Core.Members;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class MemberEmailAddressUpdateTokenRepository : WriteRepositoryBase<MemberEmailAddressUpdateToken>, IMemberEmailAddressUpdateTokenRepository
{
    public MemberEmailAddressUpdateTokenRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQuerySingleOrDefault<MemberEmailAddressUpdateToken> GetByMemberId(Guid memberId) => Set()
        .Where(x => x.MemberId == memberId)
        .DeferredSingleOrDefault();
}
