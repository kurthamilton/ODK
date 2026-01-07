using ODK.Core.Members;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IMemberPasswordResetRequestRepository : IWriteRepository<MemberPasswordResetRequest>
{
    IDeferredQuerySingleOrDefault<MemberPasswordResetRequest> GetByToken(string token);
}
