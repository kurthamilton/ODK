using ODK.Core.Members;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IMemberLocationRepository : IWriteRepository<MemberLocation>
{
    IDeferredQuerySingleOrDefault<MemberLocation> GetByMemberIdOrDefault(Guid memberId);
}
