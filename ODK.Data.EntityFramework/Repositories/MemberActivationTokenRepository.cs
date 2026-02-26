using Microsoft.EntityFrameworkCore;
using ODK.Core.Members;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class MemberActivationTokenRepository : WriteRepositoryBase<MemberActivationToken>, IMemberActivationTokenRepository
{
    public MemberActivationTokenRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQuerySingleOrDefault<MemberActivationToken> GetByMemberId(Guid memberId) => Set()
        .Where(x => x.MemberId == memberId)
        .DeferredSingleOrDefault();

    public IDeferredQuerySingleOrDefault<MemberActivationToken> GetByToken(string activationToken) => Set()
        .Where(x => x.ActivationToken == activationToken)
        .DeferredSingleOrDefault();
}