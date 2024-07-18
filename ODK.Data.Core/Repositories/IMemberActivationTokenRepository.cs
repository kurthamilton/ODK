using ODK.Core.Members;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;
public interface IMemberActivationTokenRepository : IWriteRepository<MemberActivationToken>
{
    IDeferredQuerySingleOrDefault<MemberActivationToken> GetByMemberId(Guid memberId);
    IDeferredQuerySingleOrDefault<MemberActivationToken> GetByToken(string activationToken);
}
