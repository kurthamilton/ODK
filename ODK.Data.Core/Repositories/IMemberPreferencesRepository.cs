using ODK.Core.Members;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IMemberPreferencesRepository : IWriteRepository<MemberPreferences>
{
    IDeferredQuerySingleOrDefault<MemberPreferences> GetByMemberId(Guid memberId);
}
